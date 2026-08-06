using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.ViewModels.UserDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace LegendPay.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IWalletService _walletService;
        private readonly ILegendPointService _legendPointService;
        private readonly IScheduledPaymentService _scheduledPaymentService;
        private readonly ILogger<HomeController> _logger;


        public HomeController(
                IEmailService emailService,
                IOtpService otpService,
                IAuthService authService,
                IWalletService walletService,
                ILegendPointService legendPointService,
                IScheduledPaymentService scheduledPaymentService,
                ILogger<HomeController> logger)
        {
            _authService = authService;
            _walletService = walletService;
            _legendPointService = legendPointService;
            _scheduledPaymentService = scheduledPaymentService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> FundWallet()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrEmpty(user.AccountNumber))
            {
                await _authService.TryProvisionWalletAsync(user);
            }

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 10);
            var ledgerBalance = await _authService.GetLedgerBalanceAsync(user);

            var model = new WalletDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CustomerId = user.CustomerId ?? string.Empty,
                AccountNumber = user.AccountNumber ?? wallet?.AccountNumber ?? string.Empty,
                BankName = user.BankName ?? wallet?.BankName ?? string.Empty,
                Balance = ledgerBalance,
                RecentTransactions = wallet?.WalletTransactions?
                    .Select(t => new RecentTransactionViewModel
                    {
                        TransactionId = t.ExternalReference ?? t.Id.ToString(),
                        Description = t.Description ?? t.Source ?? t.Type,
                        Amount = t.Amount,
                        Type = t.Type,
                        Date = t.CreatedAt,
                        Status = t.Status
                    })
                    .ToList() ?? new List<RecentTransactionViewModel>()
            };

            ViewData["KycTier"] = model.KycTier;

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> PayBills()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = new PayBillsViewModel
            {
                AvailableBalance = await _authService.GetLedgerBalanceAsync(user),
                RecentFavorites = new List<RecentBillerViewModel>()
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> History(string? range, string? biller, string? amount, int page = 1)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _authService.GetBillHistoryAsync(user.Id, range, biller, amount, page, 10);

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Receipt(Guid id)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _authService.GetBillReceiptAsync(id, user.Id);
            if (model == null) return RedirectToAction("History");

            return View(model);
        }

        public IActionResult Onboarding()
        {
            return View();
        }
        // this means only authenticated users can access this page
        [Authorize]
        public async Task<IActionResult> HomePage()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrEmpty(user.AccountNumber))
            {
                await _authService.TryProvisionWalletAsync(user);
            }

            var model = await _authService.GetUserDashboardAsync(user);

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Subscriptions()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _authService.GetSubscriptionsAsync(user.Id);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleAutoPay(Guid id, bool enabled)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Json(new { success = false });

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return Json(new { success = false });

            var success = await _authService.SetAutoPayAsync(id, user.Id, enabled);
            return Json(new { success });
        }

        [Authorize]
        public async Task<IActionResult> LegendPoints()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _legendPointService.GetUserPointsAsync(user.Id);
            model.FirstName = user.FirstName;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RedeemPoints(int points)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var (success, message) = await _legendPointService.RedeemAsync(user.Id, points);
            TempData[success ? "RedeemSuccess" : "RedeemError"] = message;

            return RedirectToAction("LegendPoints");
        }

        [Authorize]
        public async Task<IActionResult> ScheduledPayments()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _scheduledPaymentService.GetUserSchedulesAsync(user.Id);
            model.FirstName = user.FirstName;
            model.AvailableBalance = await _authService.GetLedgerBalanceAsync(user);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSchedule(CreateScheduledPaymentViewModel form)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                var (success, message) = await _scheduledPaymentService.CreateAsync(user.Id, form);
                if (success)
                {
                    TempData["ScheduleSuccess"] = message;
                    return RedirectToAction("ScheduledPayments");
                }
                ModelState.AddModelError(string.Empty, message);
            }

            var model = await _scheduledPaymentService.GetUserSchedulesAsync(user.Id);
            model.FirstName = user.FirstName;
            model.AvailableBalance = await _authService.GetLedgerBalanceAsync(user);
            model.Form = form;

            return View("ScheduledPayments", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CancelSchedule(Guid id)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var (success, message) = await _scheduledPaymentService.CancelAsync(id, user.Id);
            TempData[success ? "ScheduleSuccess" : "ScheduleError"] = message;

            return RedirectToAction("ScheduledPayments");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            return View(new UserProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AccountNumber = user.AccountNumber,
                BankName = user.BankName,
                CustomerId = user.CustomerId,
                IsEmailVerified = user.IsEmailVerified,
                MemberSince = user.CreatedAt
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string phoneNumber)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var (success, message) = await _authService.UpdateProfileAsync(user.Id, firstName, lastName, phoneNumber);
            TempData[success ? "ProfileSuccess" : "ProfileError"] = message;

            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (newPassword != confirmPassword)
            {
                TempData["PasswordError"] = "The new passwords don't match.";
                return RedirectToAction("Profile");
            }

            var (success, message) = await _authService.ChangePasswordAsync(user.Id, currentPassword, newPassword);
            TempData[success ? "PasswordSuccess" : "PasswordError"] = message;

            return RedirectToAction("Profile");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
