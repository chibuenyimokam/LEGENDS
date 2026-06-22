using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models.Data;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using LegendPay.Services;
using LegendPay.Services.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace LegendPay.Controllers
{
    public class AuthController : Controller
    {
        // References to the database context and injected services
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IAuthService _authService;
        private readonly IWalletService _walletService;
        private readonly ILogger<AuthController> _logger;


        public AuthController(
                IEmailService emailService,
                IOtpService otpService,
                IAuthService authService,
                IWalletService walletService,
                ILogger<AuthController> logger)
        {
            _emailService = emailService;
            _otpService = otpService;
            _authService = authService;
            _walletService = walletService;
            _logger = logger;
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

                var otp = _otpService.GenerateOtp();
                //_otpService.ConfigureUserOtp(account, otp);

                try
                {
                    var user = await _authService.CreateAndSaveUserAsync(model, otp); 

                    await _emailService.SendOtpEmailAsync(model.Email, otp);

                    TempData["VerificationEmail"] = model.Email;

                    return RedirectToAction("VerifyEmail"); 
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
            var email = TempData["VerificationEmail"] as string; 

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("SignUp");
            }

            var model = new VerifyEmailViewModel { Email = email }; 
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var isValid = await _otpService.ValidateUserOtpAsync(model.Email, model.OtpCode);

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid or expired OTP");
                return View(model);
            }

            return RedirectToAction("Login"); 
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
            var newOtp = _otpService.GenerateOtp();

            _otpService.ConfigureUserOtp(account, newOtp);

            await _emailService.SendOtpEmailAsync(account.Email, newOtp);

            TempData["VerificationEmail"] = account.Email;

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
                
                var user = await _authService.ValidateLoginCredentialsAsync(model.PhoneNumberOrEmail, model.Password); 
                if (user != null)   
                {
                    if (!user.IsEmailVerified)
                    {
                        // Store email in TempData and redirect to verify if account is unverified (this is a bad architectural choice but it works for this demo rn)
                        TempData["VerificationEmail"] = user.Email;
                        return RedirectToAction("VerifyEmail");
                    }

                    await _authService.SignInUserAsync(HttpContext, user);

                    return RedirectToAction("HomePage", "Home"); 
                }
                else
                {
                    _logger.LogError("Invalid login attempt for email: {Email}", model.PhoneNumberOrEmail);
                    ModelState.AddModelError("", "Invalid email or password");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutUserAsync(HttpContext); 
            return RedirectToAction("Login"); 
        }

    }
}