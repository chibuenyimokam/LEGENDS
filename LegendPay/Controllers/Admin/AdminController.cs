using LegendPay.Interfaces.Admin;
using LegendPay.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LegendPay.Controllers.Admin
{
    public class AdminController : Controller
    {
        private readonly IAdminAuthService _adminAuthService;

        public AdminController(IAdminAuthService adminAuthService)
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
                var response = await _adminAuthService.VerifyTwoFactorAsync(email, twoFactorCode, HttpContext);

                if (response.Success)
                    return RedirectToAction("Dashboard");

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
        public async Task<IActionResult> Logout()
        {
            await _adminAuthService.LogoutAsync(HttpContext);
            return RedirectToAction("Login");
        }
    }
}