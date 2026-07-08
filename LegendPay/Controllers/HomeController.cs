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

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);
            var categoriesResponse = await _billerOneService.GetCategoriesAsync();
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

            var model = new PayBillsViewModel
            {
                AvailableBalance = wallet?.Balance ?? 0m,
                RecentFavorites = new List<RecentBillerViewModel>()
            };

            return View("~/Views/Home/Electricity/ElectricityBillers.cshtml", model);
        }

        [Authorize]
        public async Task<IActionResult> ElectricityDetails(string billerName, string billerFullName)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

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

            return View("~/Views/Home/Electricity/ElectricityDetails.cshtml", model);
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

            return View("~/Views/Home/Electricity/ElectricityReview.cshtml", model);
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
                UnitValue = Math.Round(model.Amount / 86, 1),
                PointsEarned = (int)(model.Amount * 0.02m)
            };

            return View("~/Views/Home/Electricity/ElectricitySuccess.cshtml", successModel);
        }

       

        [Authorize]
        public async Task<IActionResult> AirtimeDetails()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = new AirtimeDetailsViewModel
            {
                CustomerName = $"{user.FirstName} {user.LastName}"
            };

            return View("~/Views/Home/AirTime/AirtimeDetails.cshtml", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AirtimeReview(
            string network, string phoneNumber, decimal amount, bool saveBeneficiary)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);

            var model = new AirtimeReviewViewModel
            {
                Network = network,
                PhoneNumber = phoneNumber,
                Amount = amount,
                WalletBalance = wallet?.Balance ?? 0m,
                SaveBeneficiary = saveBeneficiary
            };

            return View("~/Views/Home/AirTime/AirtimeReview.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AirtimePayment(AirtimeReviewViewModel model)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);
            var balance = wallet?.Balance ?? 0m;

            if (balance < model.Amount)
            {
                TempData["PaymentError"] = "Your wallet balance is not sufficient for this transaction. Please fund your wallet and try again.";
                return RedirectToAction("AirtimeReview", new
                {
                    network = model.Network,
                    phoneNumber = model.PhoneNumber,
                    amount = model.Amount,
                    saveBeneficiary = model.SaveBeneficiary
                });
            }

            // TODO: Replace mock below with real airtime API call when integrated
            var rng = new Random();
            var successModel = new AirtimeSuccessViewModel
            {
                Network = model.Network,
                PhoneNumber = model.PhoneNumber,
                Amount = model.Amount,
                PaidAt = DateTime.Now,
                TransactionRef = $"LP-{rng.Next(1000000, 9999999)}-X",
                PointsEarned = (int)(model.Amount * 0.02m)
            };

            return View("~/Views/Home/AirTime/AirtimeSuccess.cshtml", successModel);
        }

        

        [Authorize]
        public async Task<IActionResult> InternetDetails()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = new InternetDetailsViewModel
            {
                CustomerName = $"{user.FirstName} {user.LastName}"
            };

            return View("~/Views/Home/Internet/InternetDetails.cshtml", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> InternetReview(
            string network, string planLabel, string planDuration,
            string phoneNumber, decimal amount, bool saveBeneficiary)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);

            var model = new InternetReviewViewModel
            {
                Network = network,
                PlanLabel = planLabel,
                PlanDuration = planDuration,
                PhoneNumber = phoneNumber,
                Amount = amount,
                WalletBalance = wallet?.Balance ?? 0m,
                SaveBeneficiary = saveBeneficiary
            };

            return View("~/Views/Home/Internet/InternetReview.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> InternetPayment(InternetReviewViewModel model)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);
            var balance = wallet?.Balance ?? 0m;

            if (balance < model.Amount)
            {
                TempData["PaymentError"] = "Your wallet balance is not sufficient for this transaction. Please fund your wallet and try again.";
                return RedirectToAction("InternetReview", new
                {
                    network = model.Network,
                    planLabel = model.PlanLabel,
                    planDuration = model.PlanDuration,
                    phoneNumber = model.PhoneNumber,
                    amount = model.Amount,
                    saveBeneficiary = model.SaveBeneficiary
                });
            }

            // TODO: Replace mock below with real data API call when integrated
            var rng = new Random();
            var successModel = new InternetSuccessViewModel
            {
                Network = model.Network,
                PlanLabel = model.PlanLabel,
                PlanDuration = model.PlanDuration,
                PhoneNumber = model.PhoneNumber,
                Amount = model.Amount,
                PaidAt = DateTime.Now,
                TransactionRef = $"LP-{rng.Next(1000000, 9999999)}-X",
                PointsEarned = (int)(model.Amount * 0.02m)
            };

            return View("~/Views/Home/Internet/InternetSuccess.cshtml", successModel);
        }

        // ── DIGITAL TV ───────────────────────────────────────────────────────────────

        [Authorize]
        public async Task<IActionResult> DigitalTVBillers()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            return View("~/Views/Home/DigitalTV/DigitalTVBillers.cshtml");
        }

        [Authorize]
        public async Task<IActionResult> DigitalTVDetails(string providerName, string providerFullName)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = new DigitalTVDetailsViewModel
            {
                ProviderName = providerName,
                ProviderFullName = providerFullName,
                CustomerName = $"{user.FirstName} {user.LastName}"
            };

            return View("~/Views/Home/DigitalTV/DigitalTVDetails.cshtml", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DigitalTVReview(
            string providerName, string providerFullName, string smartcardNumber,
            string customerName, string packageLabel, decimal amount, bool saveBeneficiary)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);

            var model = new DigitalTVReviewViewModel
            {
                ProviderName = providerName,
                ProviderFullName = providerFullName,
                SmartcardNumber = smartcardNumber,
                CustomerName = customerName,
                PackageLabel = packageLabel,
                Amount = amount,
                WalletBalance = wallet?.Balance ?? 0m,
                SaveBeneficiary = saveBeneficiary
            };

            return View("~/Views/Home/DigitalTV/DigitalTVReview.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DigitalTVPayment(DigitalTVReviewViewModel model)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);
            var balance = wallet?.Balance ?? 0m;

            if (balance < model.Amount)
            {
                TempData["PaymentError"] = "Your wallet balance is not sufficient for this transaction. Please fund your wallet and try again.";
                return RedirectToAction("DigitalTVReview", new
                {
                    providerName = model.ProviderName,
                    providerFullName = model.ProviderFullName,
                    smartcardNumber = model.SmartcardNumber,
                    customerName = model.CustomerName,
                    packageLabel = model.PackageLabel,
                    amount = model.Amount,
                    saveBeneficiary = model.SaveBeneficiary
                });
            }

            // TODO: Replace mock below with real TV subscription API call when integrated
            var rng = new Random();
            var successModel = new DigitalTVSuccessViewModel
            {
                ProviderName = model.ProviderName,
                ProviderFullName = model.ProviderFullName,
                SmartcardNumber = model.SmartcardNumber,
                PackageLabel = model.PackageLabel,
                Amount = model.Amount,
                PaidAt = DateTime.Now,
                TransactionRef = $"LP-{rng.Next(1000000, 9999999)}-X",
                PointsEarned = (int)(model.Amount * 0.02m)
            };

            return View("~/Views/Home/DigitalTV/DigitalTVSuccess.cshtml", successModel);
        }
    }
}