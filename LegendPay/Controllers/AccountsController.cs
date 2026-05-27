using LegendPay.Models;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using LegendPay.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace LegendPay.Controllers
{
    public class AccountsController : Controller
    {
        //the refenrence to the database context
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        //constructor to inject the database context into the controller
        //constructor dependency injection to get an instance of AppDbContext (the db context object)
        public AccountsController(AppDbContext appDBcontext, EmailService emailService)
        {
            _context = appDBcontext; 
            _emailService = emailService;
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
            if(ModelState.IsValid)
            {
                // Process the sign-up data (e.g., save to database)
                // Redirect to a success page or display a success message
                //return RedirectToAction("Index", "Home");
                UserAccount account = new UserAccount();

                    account.FirstName = model.FirstName;
                    account.LastName = model.LastName;
                    account.Email = model.Email;
                    account.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    account.PhoneNumber = model.PhoneNumber;

                // Generate OTP code and set expiration time
                var otp = new Random().Next(100000, 999999).ToString(); // Generate a 6-digit OTP
                account.OtpCode = otp;
                account.OtpExpiration = DateTime.UtcNow.AddMinutes(10); // OTP expires in 10 minutes
                account.IsEmailVerified = false; // Set email verification status to false

                try
                {
                    _context.UserAccounts.Add(account); //to add the new account to the database context
                    await _context.SaveChangesAsync(); // so that the changes are saved to the database

                    //send otp to user's email
                    await _emailService.SendOtpEmailAsync(account.Email, otp);

                    //store email in session for verification page
                    TempData["VerifiactionEmail"] = account.Email; //to store the email in TempData so that it can be accessed in the verification page

                    return RedirectToAction("VerifyEmail"); //redirect to the email verification page after successful sign-up

                    //ModelState.Clear(); // Clear the form data after successful submission
                    //ViewBag.Message = $"Account for {account.FirstName} {account.LastName} has been created successfully. Please login";


                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Email already exists");

                    return View(model);
                }
                
                //return View();
            }
            return View(model);
         }


        public IActionResult VerifyEmail()
        {
            //to pass the email stored in TempData to the view using ViewBag
            ViewBag.Email = TempData["VerifiactionEmail"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(string email, string otp)
        { 
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == email); //find the user by email

            if (user == null || user.OtpCode != otp || user.OtpExpiration < DateTime.UtcNow)
            {
                ViewBag.Email = email; //to pass the email back to the view in case of an error
                ModelState.AddModelError("", "Invalid or expired OTP");
                return View();
            }

            //mark as verified
            user.IsEmailVerified = true;
            user.OtpCode = null; //clear the OTP code and expiration time after successful verification
            user.OtpExpiration = null;
            await _context.SaveChangesAsync(); //save the changes to the database


            return RedirectToAction("Login"); //redirect to the login page after successful email verification
        }


        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid) 
            {
                var user = _context.UserAccounts.Where(u => u.Email == model.PhoneNumberOrEmail || u.PhoneNumber == model.PhoneNumberOrEmail).FirstOrDefault(); // finds user first then verify password seperately
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password)) //password is correct? proceed with login
                {
                    // User found, redirect to a different page or perform login actions
                    //return RedirectToAction("Index", "Home");

                    if (!user.IsEmailVerified)
                    {
                        TempData["VerificationEmail"] = user.Email; //to store the email in TempData so that it can be accessed in the verification page
                        return RedirectToAction("VerifyEmail"); //redirect to the email verification page if the email is not verified
                    }

                    //successful login, redirect to the home page
                    // In a real application, you would typically set up authentication cookies or tokens here
                    // For demonstration purposes, we'll just redirect to a secure page
                    // Create claims for the authenticated user

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim("Name", user.FirstName),
                        new Claim(ClaimTypes.Role, "User") // You can add more claims as needed
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)); //logins user and creates an authentication cookie

                    return RedirectToAction("SecurePage"); //securepage is the home page for authenticated users
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password");
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //to log out the user and clear the authentication cookie
            return RedirectToAction("Login"); //redirect to login page after logout
        }

        //only authenticated users can access this page
        [Authorize]
        public IActionResult SecurePage() //basically home page for authenticated users
        {
            ViewBag.Name = HttpContext.User.Identity.Name; //to get the name of the authenticated user and pass it to the view using ViewBag
            return View();
        }
    }
}
