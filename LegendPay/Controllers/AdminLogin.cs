using LegendPay.Interfaces.Admin;
using LegendPay.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LegendPay.Controllers
{
    public class AdminLoginController : Controller
    {
        private readonly IAdminAuthService _adminAuthService;

        public AdminLoginController(IAdminAuthService adminAuthService)
        {
            _adminAuthService = adminAuthService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Admin/AdminLogin.cshtml", new AdminLoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/AdminLogin.cshtml", model);

            try
            {
                var response = await _adminAuthService.LoginAsync(model);

                if (response.Success)
                {
                    ViewBag.ShowOtp = true;
                    ViewBag.AdminEmail = response.Data;
                    ViewBag.SuccessMessage = response.Message;
                    return View("~/Views/Admin/AdminLogin.cshtml", model);
                }

                ViewBag.ErrorMessage = response.Message;
                return View("~/Views/Admin/AdminLogin.cshtml", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Something went wrong. Please try again.";
                return View("~/Views/Admin/AdminLogin.cshtml", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string twoFactorCode)
        {
            try
            {
                var response = await _adminAuthService.VerifyTwoFactorAsync(email, twoFactorCode);

                if (response.Success)
                {
                    Response.Cookies.Append("AdminToken", response.Data, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    return RedirectToAction("Dashboard", "AdminLogin");
                }

                ViewBag.ErrorMessage = response.Message;
                ViewBag.ShowOtp = true;
                ViewBag.AdminEmail = email;
                return View("~/Views/Admin/AdminLogin.cshtml", new AdminLoginViewModel());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Something went wrong. Please try again.";
                return View("~/Views/Admin/AdminLogin.cshtml", new AdminLoginViewModel());
            }
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View("~/Views/Admin/Dashboard.cshtml");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AdminToken");
            return RedirectToAction("Login", "AdminLogin");
        }
    }
}