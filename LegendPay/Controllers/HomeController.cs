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
        private readonly ILogger<HomeController> _logger;


        public HomeController(
                IEmailService emailService,
                IOtpService otpService,
                IAuthService authService,
                IWalletService walletService,
                ILogger<HomeController> logger)
        {
            _authService = authService;
            _walletService = walletService;
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
        public IActionResult Onboarding()
        {
            return View();
        }
        // this means only authenticated users can access this page
        [Authorize]
        public async Task<IActionResult> HomePage()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrEmpty(user.AccountNumber))
            {
                await _authService.TryProvisionWalletAsync(user);
            }

            var model = await _authService.GetUserDashboardAsync(user);

            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> FundWallet()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 10);

            var model = new WalletDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CustomerId = user.CustomerId ?? string.Empty,
                AccountNumber = wallet?.AccountNumber ?? user.AccountNumber ?? string.Empty,
                BankName = wallet?.BankName ?? user.BankName ?? string.Empty,
                WalletBalance = user.Balance,
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
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);

            var model = new PayBillsViewModel
            {
                AvailableBalance = wallet?.Balance ?? 0m,
                RecentFavorites = new List<RecentBillerViewModel>()
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> History(string? range, string? biller, string? amount, int page = 1)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _authService.GetBillHistoryAsync(user.Id, range, biller, amount, page, 10);

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Receipt(Guid id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _authService.GetBillReceiptAsync(id, user.Id);
            if (model == null) return RedirectToAction("History");

            return View(model);
        }
       

        [Authorize]
        public async Task<IActionResult> Subscriptions()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _authService.GetSubscriptionsAsync(user.Id);

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}