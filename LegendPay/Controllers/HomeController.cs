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

        private readonly IBillerOneService _billerOneService;
        


        public HomeController(
                IAuthService authService,
                IWalletService walletService,
                IBillerOneService billerOneService,
                ILogger<HomeController> logger)
        {
            _authService = authService;
            _walletService = walletService;
            _logger = logger;
            _billerOneService = billerOneService;
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

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 10);

            var model = new WalletDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CustomerId = user.CustomerId ?? string.Empty,
                AccountNumber = wallet?.StaticAccountNumber ?? user.AccountNumber ?? string.Empty,
                BankName = wallet?.BankName ?? user.BankName ?? string.Empty,
                Balance = wallet?.Balance ?? 0m,
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

            if (string.IsNullOrEmpty(user.AccountNumber))
            {
                await _authService.TryProvisionWalletAsync(user);
            }

            var model = await _authService.GetUserDashboardAsync(user);
            var categoriesResponse = await _billerOneService.GetCategoriesAsync();
            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);
            var billersResponse = await _billerOneService.GetBillersAsync();

            var model = new PayBillsViewModel
            {
                AvailableBalance = wallet?.Balance ?? 0m,
                RecentFavorites = new List<RecentBillerViewModel>(),
                Categories = categoriesResponse?.CategoryList?
                .Select(c => new BillerCategoryViewModel
                {
                    Category = c.Category,
                    LogoUrl = c.LogoUrl,
                    IconName = MapCategoryToIcon(c.Category)
                })
                .ToList() ?? new List<BillerCategoryViewModel>(),

                Billers = billersResponse?.Billers?
                .Select(b => new BillerViewModel
                {
                    Category = b.Category,
                    BillerName = b.BillerName,
                    BillerId = b.BillerId,
                    LogoPath = b.LogoPath,
                    Description = b.Description,
                    AmountInVerification = b.AmountInVerification,
                    ReferenceIdVerifiable = b.ReferenceIdVerifiable
                })
                .ToList() ?? new List<BillerViewModel>()
            };

            return View(model);
        }
        // Helper method to map BillerOne categories to Material Icons
        private string MapCategoryToIcon(string category)
        {
            return category?.ToUpper() switch
            {
                "ELECTRICITY" => "bolt",
                "AIRTIME" => "smartphone",
                "DIGITALTV" => "tv",
                "GAMES" => "sports_esports",
                "EDUCATION" => "school",
                "TRANSPORT" => "commute",
                "INTERNATIONAL AIRTIME" => "public",
                "EVENTS AND LIFESTYLE" => "event",
                "INSURANCE" => "shield",
                _ => "receipt_long"
            };
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
        public async Task<IActionResult> Subscriptions()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");
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


        [Authorize]
        public async Task<IActionResult> ElectricityBillers()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);

            // Reuse PayBillsViewModel just for the wallet balance (same pattern as PayBills action)
            var model = new PayBillsViewModel
            {
                AvailableBalance = wallet?.Balance ?? 0m,
                RecentFavorites = new List<RecentBillerViewModel>()
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> ElectricityDetails(string billerName, string billerFullName)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            // Map biller short name to location
            var locationMap = new Dictionary<string, string>
    {
        { "IKEDC",  "Lagos, Nigeria"         },
        { "EKEDC",  "Lagos, Nigeria"         },
        { "AEDC",   "Abuja, Nigeria"         },
        { "IBEDC",  "Ibadan, Nigeria"        },
        { "EEDC",   "Enugu, Nigeria"         },
        { "PHEDC",  "Port Harcourt, Nigeria" },
        { "KAEDCO", "Kaduna, Nigeria"        },
        { "JEDC",   "Jos, Nigeria"           },
        { "BEDC",   "Benin City, Nigeria"    },
        { "YEDC",   "Yola, Nigeria"          },
        { "KEDCO",  "Kano, Nigeria"          },
    };

            var model = new ElectricityDetailsViewModel
            {
                BillerName = billerName,
                BillerFullName = billerFullName,
                BillerLocation = locationMap.TryGetValue(billerName, out var loc) ? loc : "Nigeria",
                CustomerName = $"{user.FirstName} {user.LastName}"
            };

            return View(model);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ElectricityReview(
            string billerName, string billerFullName, string billerLocation,
            string meterNumber, string customerName, decimal amount, bool saveBeneficiary)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);

            var model = new ElectricityReviewViewModel
            {
                BillerName = billerName,
                BillerFullName = billerFullName,
                BillerLocation = billerLocation,
                MeterNumber = meterNumber,
                CustomerName = customerName,
                Amount = amount,
                WalletBalance = wallet?.Balance ?? 0m,
                SaveBeneficiary = saveBeneficiary
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ElectricityPayment(ElectricityReviewViewModel model)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);
            var balance = wallet?.Balance ?? 0m;

            if (balance < model.Amount)
            {
                // Redirect back to review with error
                TempData["PaymentError"] = "Your wallet balance is not sufficient for this transaction. Please fund your wallet and try again.";
                return RedirectToAction("ElectricityReview", new
                {
                    billerName = model.BillerName,
                    billerFullName = model.BillerFullName,
                    billerLocation = model.BillerLocation,
                    meterNumber = model.MeterNumber,
                    customerName = model.CustomerName,
                    amount = model.Amount,
                    saveBeneficiary = model.SaveBeneficiary
                });
            }

            
            // TODO: Replace mock below with real payment API call when integrated
            // e.g: var result = await _billPaymentService.PayElectricityAsync
            // MOCK — generate fake token and transaction ref

            var rng = new Random();
            string GenerateToken() =>
                string.Join(" ", Enumerable.Range(0, 5).Select(_ => rng.Next(1000, 9999).ToString()));

            var successModel = new ElectricitySuccessViewModel
            {
                BillerName = model.BillerName,
                BillerFullName = model.BillerFullName,
                MeterNumber = model.MeterNumber,
                Amount = model.Amount,
                PaidAt = DateTime.Now,
                TransactionRef = $"LP-{rng.Next(1000000, 9999999)}-X",
                ElectricityToken = GenerateToken(),
                UnitValue = Math.Round(model.Amount / 86, 1),  // rough kWh estimate
                PointsEarned = (int)(model.Amount * 0.02m)
            };
            // END MOCK

            return View("ElectricitySuccess", successModel);
        }

    }
}