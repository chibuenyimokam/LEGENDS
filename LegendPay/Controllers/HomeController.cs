using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.ViewModels;
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

        [Authorize]
        public async Task<IActionResult> HomePage()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrEmpty(user.AccountNumber))
            {
                bool provisionSuccess = await _authService.TryProvisionWalletAsync(user);
                if (!provisionSuccess)
                {
                    ViewBag.FullName = $"{user.FirstName} {user.LastName}";
                    ViewBag.Balance = "Unavailable";
                    ViewBag.AccountNumber = "Pending Activation";
                    ViewBag.CustomerId = "Pending Activation";
                    ViewBag.BankName = "Unavailable";
                    return View();
                }
            }

            var balance = await _authService.GetUserBalanceAsync(user.Email);

            ViewBag.FullName = $"{user.FirstName} {user.LastName}";
            ViewBag.Balance = balance.HasValue ? balance.Value.ToString("N2") : "0.00";
            ViewBag.AccountNumber = user.AccountNumber;
            ViewBag.BankName = user.BankName;
            ViewBag.CustomerId = user.CustomerId;

            return View("HomePage");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null)
                return RedirectToAction("Login");

            var balance = await _walletService.GetBalanceAsync(user.CustomerId);

            var model = new UserDashboardViewModel
            {
                AccountInfo = new AccountInfoViewModel
                {
                    UserName = $"{user.FirstName} {user.LastName}",
                    AvailableBalance = balance ?? 0,
                    WalletId = user.WalletId ?? "N/A"
                },
                LegendPoints = new LegendPointsViewModel
                {
                    CurrentPoints = user.LegendPoint?.TotalPoints ?? 0,
                    GoalPoints = 5000,
                    AmountToNextReward = 500
                },
                RecentActivities = new List<RecentActivityViewModel>(),
                UpcomingRenewals = new List<UpcomingRenewalViewModel>()
            };

            return View("HomePage", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PayBills()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            var balance = await _walletService.GetBalanceAsync(user.CustomerId);

            var model = new PayBillsViewModel
            {
                UserName = $"{user.FirstName} {user.LastName}",
                WalletId = user.WalletId ?? "N/A",
                AvailableBalance = balance ?? 0,

                // Empty until SavedBillers/Favorites table is built
                RecentFavorites = new List<RecentBillerViewModel>(),

                Categories = new List<BillCategoryViewModel>
                {
                    new() {
                        Name = "Airtime", ColorClass = "cat-pink",
                        SvgIcon = "<svg fill='none' stroke='currentColor' stroke-width='2' viewBox='0 0 24 24'><rect x='5' y='2' width='14' height='20' rx='2'/><line x1='12' y1='18' x2='12' y2='18' stroke-linecap='round' stroke-width='3'/></svg>"
                    },
                    new() {
                        Name = "Internet/Data", ColorClass = "cat-blue",
                        SvgIcon = "<svg fill='none' stroke='currentColor' stroke-width='2' viewBox='0 0 24 24'><path d='M5 12.55a11 11 0 0114.08 0'/><path d='M1.42 9a16 16 0 0121.16 0'/><path d='M8.53 16.11a6 6 0 016.95 0'/><circle cx='12' cy='20' r='1' fill='currentColor'/></svg>"
                    },
                    new() {
                        Name = "Electricity", ColorClass = "cat-yellow",
                        SvgIcon = "<svg fill='none' stroke='currentColor' stroke-width='2' viewBox='0 0 24 24'><polygon points='13 2 3 14 12 14 11 22 21 10 12 10 13 2'/></svg>"
                    },
                    new() {
                        Name = "Digital TV", ColorClass = "cat-purple",
                        SvgIcon = "<svg fill='none' stroke='currentColor' stroke-width='2' viewBox='0 0 24 24'><rect x='2' y='7' width='20' height='15' rx='2'/><polyline points='17 2 12 7 7 2'/></svg>"
                    },
                    new() {
                        Name = "Games", ColorClass = "cat-violet",
                        SvgIcon = "<svg fill='none' stroke='currentColor' stroke-width='2' viewBox='0 0 24 24'><line x1='6' y1='12' x2='10' y2='12'/><line x1='8' y1='10' x2='8' y2='14'/><circle cx='15' cy='11' r='1' fill='currentColor'/><circle cx='17' cy='13' r='1' fill='currentColor'/><path d='M6 20h12a2 2 0 002-2V8a2 2 0 00-2-2H6a2 2 0 00-2 2v10a2 2 0 002 2z'/></svg>"
                    },
                    new() {
                        Name = "Education", ColorClass = "cat-green",
                        SvgIcon = "<svg fill='none' stroke='currentColor' stroke-width='2' viewBox='0 0 24 24'><path d='M22 10v6M2 10l10-5 10 5-10 5z'/><path d='M6 12v5c3 3 9 3 12 0v-5'/></svg>"
                    },
                    new() {
                        Name = "Transport", ColorClass = "cat-red",
                        SvgIcon = "<svg fill='none' stroke='currentColor' stroke-width='2' viewBox='0 0 24 24'><rect x='1' y='3' width='15' height='13' rx='2'/><path d='M16 8h4l3 3v5h-7V8z'/><circle cx='5.5' cy='18.5' r='2.5'/><circle cx='18.5' cy='18.5' r='2.5'/></svg>"
                    },
                    new() {
                        Name = "Other Utilities", ColorClass = "cat-gray",
                        SvgIcon = "<svg fill='none' stroke='currentColor' stroke-width='2' viewBox='0 0 24 24'><circle cx='12' cy='12' r='1' fill='currentColor'/><circle cx='19' cy='12' r='1' fill='currentColor'/><circle cx='5' cy='12' r='1' fill='currentColor'/></svg>"
                    },
                }
            };

            return View(model);
        }
    }
}