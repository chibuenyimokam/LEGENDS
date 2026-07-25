using LegendPay.Interfaces.Admin;
using LegendPay.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LegendPay.Controllers.Admin
{
    [Authorize(AuthenticationSchemes = "AdminScheme")]
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminAuthService _adminAuthService;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IAdminUserService _adminUserService;
        private readonly IAdminTransactionService _adminTransactionService;
        private readonly IAdminReportService _adminReportService;
        private readonly IAdminSettingsService _adminSettingsService;

        public AdminController(
            IAdminAuthService adminAuthService,
            IAdminDashboardService adminDashboardService,
            IAdminUserService adminUserService,
            IAdminTransactionService adminTransactionService,
            IAdminReportService adminReportService,
            IAdminSettingsService adminSettingsService)
        {
            _adminAuthService = adminAuthService;
            _adminDashboardService = adminDashboardService;
            _adminUserService = adminUserService;
            _adminTransactionService = adminTransactionService;
            _adminReportService = adminReportService;
            _adminSettingsService = adminSettingsService;
        }
        [AllowAnonymous]
        [HttpGet("Admin/Login")]
        public IActionResult Login()
        {
            return View("~/Views/Admin/AdminLogin.cshtml", new AdminLoginViewModel());
        }
        [AllowAnonymous]
        [HttpPost("Admin/Login")]
        [ValidateAntiForgeryToken]
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
        [AllowAnonymous]
        [HttpPost("Admin/VerifyOtp")]
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
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View("~/Views/Admin/AdminForgotPassword.cshtml", new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/AdminForgotPassword.cshtml", model);

            await _adminAuthService.ForgotPasswordAsync(model.Email);

            TempData["AdminResetEmail"] = model.Email;
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = TempData["AdminResetEmail"] as string;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("ForgotPassword");

            return View("~/Views/Admin/AdminResetPassword.cshtml", new ResetPasswordViewModel { Email = email });
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/AdminResetPassword.cshtml", model);

            var response = await _adminAuthService.ResetPasswordAsync(model.Email, model.OtpCode, model.NewPassword);
            if (!response.Success)
            {
                ModelState.AddModelError("", response.Message);
                return View("~/Views/Admin/AdminResetPassword.cshtml", model);
            }

            TempData["AdminPasswordReset"] = true;
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var model = await _adminDashboardService.GetDashboardAsync();
            return View("~/Views/Admin/Dashboard.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UserRegistry(string? search, string? status, decimal? minBalance, int page = 1)
        {
            var model = await _adminUserService.GetUserRegistryAsync(search, status, minBalance, page, 15);
            return View("~/Views/Admin/UserRegistry.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Transactions(string? status, string? biller, string? method, int page = 1)
        {
            var model = await _adminTransactionService.GetTransactionsAsync(status, biller, method, page, 15);
            return View("~/Views/Admin/Transactions.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Reports()
        {
            var model = await _adminReportService.GetReportsAsync();
            return View("~/Views/Admin/Reports.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> LegendPoints()
        {
            var model = await _adminSettingsService.GetLegendPointsAsync();
            return View("~/Views/Admin/LegendPoints.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> LegendPoints(AdminLegendPointsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var reloaded = await _adminSettingsService.GetLegendPointsAsync();
                model.TotalPointsIssued = reloaded.TotalPointsIssued;
                model.TotalRedeemedPoints = reloaded.TotalRedeemedPoints;
                model.ActiveUsers = reloaded.ActiveUsers;
                model.UpdatedAt = reloaded.UpdatedAt;
                return View("~/Views/Admin/LegendPoints.cshtml", model);
            }

            Guid? adminId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
            await _adminSettingsService.UpdateLegendPointsAsync(model, adminId);

            TempData["Saved"] = true;
            return RedirectToAction("LegendPoints");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _adminAuthService.LogoutAsync(HttpContext);
            return RedirectToAction("Login");
        }
    }
}