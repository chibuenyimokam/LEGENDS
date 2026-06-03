using LegendPay.Interfaces;
using LegendPay.Models.Data;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using LegendPay.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace LegendPay.Controllers
{
    public class AccountsController : Controller
    {
        // References to the database context and injected services
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IAuthService _authService;

        // Constructor dependency injection to get instances of all required services
        public AccountsController(
                IEmailService emailService,
                IOtpService otpService,
                IAuthService authService)
        {
            _emailService = emailService;
            _otpService = otpService;
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View(new SignUpViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {

                // Generate OTP and configure expiration/verification state via OtpService
                var otp = _otpService.GenerateOtp();
                //_otpService.ConfigureUserOtp(account, otp);

                try
                {
                    // delegate user creation, password hashing, OTP configuration, and database saving to AuthService
                    var user = await _authService.CreateAndSaveUserAsync(model, otp); 

                    // Send OTP to the user's email via EmailService
                    await _emailService.SendOtpEmailAsync(model.Email, otp);

                    // Store email in TempData so the verification page can access it
                    TempData["VerificationEmail"] = model.Email;

                    return RedirectToAction("VerifyEmail"); // redirect to the email verification page
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Email or unique constraint already exists");
                    return View(model);
                }
            }
            return View(model);
        }


        public IActionResult VerifyEmail()
        {
            var email = TempData["VerificationEmail"] as string; // retrieve the email stored during sign-up or login

            if (string.IsNullOrEmpty(email))
            {
                // Safety net: if they refresh or navigate here manually without an email, redirect away
                return RedirectToAction("SignUp");
            }

            var model = new VerifyEmailViewModel { Email = email }; // pre-populate the model with the email
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Delegates OTP validation (lookup, comparison, expiry check, and DB update) to OtpService
            var isValid = await _otpService.ValidateUserOtpAsync(model.Email, model.OtpCode);

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid or expired OTP");
                return View(model); // keep the email in the model so the user can retry without re-entering it
            }

            return RedirectToAction("Login"); // redirect to login after successful email verification
        }

        [HttpGet]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                email = TempData["VerificationEmail"] as string;
            }
            
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("SignUp");
            }
            var account = await _authService.GetUserByEmailAsync(email);
            if (account == null)
            {
                return RedirectToAction("SignUp");
            }
            // 1. Generate a brand new OTP
            var newOtp = _otpService.GenerateOtp();

            // 2. Configure user OTP (this overwrites account.OtpCode, discarding the old one)
            _otpService.ConfigureUserOtp(account, newOtp);

            // 3. Send out the fresh email via SendGrid
            await _emailService.SendOtpEmailAsync(account.Email, newOtp);

            // Keep the email alive in TempData for the next submission cycle
            TempData["VerificationEmail"] = account.Email;

            // Redirect back to the verification page with a success flag
            return RedirectToAction("VerifyEmail", new { resent = true });
        }


        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var user = await _authService.ValidateLoginCredentialsAsync(model.PhoneNumberOrEmail, model.Password); // delegate password verification to AuthService
                if (user != null)   
                {
                    if (!user.IsEmailVerified)
                    {
                        // Store email in TempData and redirect to verify if account is unverified, bad architectural choice but it works for this demo
                        TempData["VerificationEmail"] = user.Email;
                        return RedirectToAction("VerifyEmail");
                    }

                    // Delegate cookie sign-in (claims creation + SignInAsync) to AuthService
                    await _authService.SignInUserAsync(HttpContext, user);

                    return RedirectToAction("HomePage"); // redirect authenticated user to the home page
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutUserAsync(HttpContext); // delegate sign-out to AuthService
            return RedirectToAction("Login"); // redirect to login page after logout
        }

        // Only authenticated users can access this page
        [Authorize]
        public IActionResult HomePage()
        {
            var firstname = HttpContext.User.FindFirst(ClaimTypes.GivenName)?.Value; // get authenticated user's first name from claims
            var lastname = HttpContext.User.FindFirst(ClaimTypes.Surname)?.Value;    // get authenticated user's last name from claims

            ViewBag.FullName = $"{firstname} {lastname}"; // combine and pass full name to the view
            return View();
        }
    }
}