using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.Data.Response_Table;
using LegendPay.Models.ViewModels.UserDashboard;
using LegendPay.Models.WalletStation.Request;
using LegendPay.Models.BillerOne.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using LegendPay.Models.Data;
using LegendPay.Models.ViewModels;

namespace LegendPay.Controllers
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IWalletService _walletService;
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IBillerOneService _billerOneService;


        public HomeController(
                 IAuthService authService,
                 IWalletService walletService,
                 IBillerOneService billerOneService,
                 ILogger<HomeController> logger,
                 AppDbContext context)
        {
            _authService = authService;
            _walletService = walletService;
            _logger = logger;
            _billerOneService = billerOneService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [AllowAnonymous]
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
        [Authorize]
        public async Task<IActionResult> Beneficiaries()
        {
            var response = await _billerOneService.GetBeneficiariesAsync();

            var model = new BeneficiariesViewModel
            {
                Beneficiaries = response?.BeneficiaryList?
                    .Select(b => new BeneficiaryDisplayItem
                    {
                        BenefId = b.BenefId,
                        BenefName = b.BenefName,
                        BenefRefId = b.BenefRefId,
                        Biller = b.Biller,
                        Category = b.Category,
                        CategoryDisplayName = GetCategoryDisplayName(b.Category),
                        CategoryIcon = GetCategoryIcon(b.Category)
                    })
                    .ToList() ?? new List<BeneficiaryDisplayItem>()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBeneficiary(string benefId)
        {
            if (string.IsNullOrEmpty(benefId))
                return RedirectToAction("Beneficiaries");

            var response = await _billerOneService.DeleteBeneficiaryAsync(benefId);
            if (response?.ResponseCode != "00")
            {
                TempData["DeleteError"] = "Could not remove beneficiary. Please try again.";
            }

            return RedirectToAction("Beneficiaries");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        // Helper: map category to display name
        private string GetCategoryDisplayName(string category) => category?.ToUpper() switch
        {
            "ELECTRICITY" => "Electricity",
            "AIRTIME" => "Airtime",
            "DATA" => "Internet / Data",
            "DIGITALTV" => "Digital TV",
            "EDUCATION" => "Education",
            "GAMES" => "Games & Betting",
            "TRANSPORT" => "Transport",
            _ => category ?? "Bills"
        };

        // Helper: map category to Material Symbol icon
        private string GetCategoryIcon(string category) => category?.ToUpper() switch
        {
            "ELECTRICITY" => "bolt",
            "AIRTIME" => "smartphone",
            "DATA" => "wifi",
            "DIGITALTV" => "tv",
            "EDUCATION" => "school",
            "GAMES" => "sports_esports",
            "TRANSPORT" => "commute",
            _ => "receipt_long"
        };

        // Helper: map category to reference label
        private string GetReferenceLabel(string category, string billerName) => category?.ToUpper() switch
        {
            "ELECTRICITY" => "Meter / Account Number",
            "DIGITALTV" => "Smartcard / IUC Number",
            "EDUCATION" => billerName switch
            {
                "JAMB" => "Registration Number",
                "WAEC" => "Exam Number",
                "NECO" => "Exam Number",
                "NABTEB" => "Exam Number",
                _ => "Student ID / Application Number"
            },
            "AIRTIME" => "Phone Number",
            "DATA" => "Phone Number",
            "GAMES" => "User ID",
            "TRANSPORT" => "Account / Reference Number",
            _ => "Reference Number"
        };

        private bool IsFourStep(string category) =>
            category?.ToUpper() is "ELECTRICITY" or "DIGITALTV" or "EDUCATION";

        // Helper: map biller to electricity location
        private string GetElectricityLocation(string billerName) => billerName?.ToUpper() switch
        {
            var b when b.Contains("IKEJA") || b.Contains("IKEDC") => "Lagos, Nigeria",
            var b when b.Contains("EKO") || b.Contains("EKEDC") => "Lagos, Nigeria",
            var b when b.Contains("ABUJA") || b.Contains("AEDC") => "Abuja, Nigeria",
            var b when b.Contains("IBADAN") || b.Contains("IBEDC") => "Ibadan, Nigeria",
            var b when b.Contains("ENUGU") || b.Contains("EEDC") => "Enugu, Nigeria",
            var b when b.Contains("PORT") || b.Contains("PHEDC") => "Port Harcourt, Nigeria",
            var b when b.Contains("KADUNA") || b.Contains("KAEDCO") => "Kaduna, Nigeria",
            var b when b.Contains("JOS") || b.Contains("JEDC") => "Jos, Nigeria",
            var b when b.Contains("BENIN") || b.Contains("BEDC") => "Benin City, Nigeria",
            var b when b.Contains("YOLA") || b.Contains("YEDC") => "Yola, Nigeria",
            var b when b.Contains("KANO") || b.Contains("KEDCO") => "Kano, Nigeria",
            _ => "Nigeria"
        };

        [Authorize]
        public async Task<IActionResult> SelectBiller(string category)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var billersResponse = await _billerOneService.GetBillersAsync();

            var billers = billersResponse?.Billers?
                .Where(b => string.Equals(b.Category, category, StringComparison.OrdinalIgnoreCase))
                .Select(b => new BillerItem
                {
                    BillerId = b.BillerId,
                    BillerName = b.BillerName,
                    Description = b.Description,
                    LogoUrl = b.LogoPath,
                    ReferenceIdVerifiable = b.ReferenceIdVerifiable,
                    AmountInVerification = b.AmountInVerification
                })
                .ToList() ?? new List<BillerItem>();

            var model = new SelectBillerViewModel
            {
                Category = category,
                CategoryDisplayName = GetCategoryDisplayName(category),
                CategoryIcon = GetCategoryIcon(category),
                Billers = billers
            };

            return View("~/Views/Home/Templates/SelectBiller.cshtml", model);
        }

        [Authorize]
        public async Task<IActionResult> BillerDetails(
        string category, string billerId, string billerName,
        bool referenceIdVerifiable, bool amountInVerification)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var packages = new List<BillerPackageItem>();

            if (category?.ToUpper() == "DIGITALTV")
            {
                var packagesResponse = await _billerOneService.GetBillerPackagesAsync(billerId);
                packages = packagesResponse?.Packages?
                    .Select(p => new BillerPackageItem
                    {
                        Label = p.BillerItemName,
                        BillerItemId = p.BillerItemId,
                        Amount = p.Amount
                    })
                    .ToList() ?? new List<BillerPackageItem>();
            }

            var model = new BillerDetailsViewModel
            {
                Category = category,
                CategoryDisplayName = GetCategoryDisplayName(category),
                BillerId = billerId,
                BillerName = billerName,
                CustomerName = $"{user.FirstName} {user.LastName}",
                BillerLocation = category?.ToUpper() == "ELECTRICITY" ? GetElectricityLocation(billerName) : string.Empty,
                ReferenceIdVerifiable = referenceIdVerifiable,
                AmountInVerification = amountInVerification,
                Packages = packages
            };

            return View("~/Views/Home/Templates/BillerDetails.cshtml", model);
        }

        [Authorize]
        public async Task<IActionResult> PurchaseDetails(string category)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            List<BillerItem> billers = new();

            // For Airtime & Data, networks are hardcoded (not from BillerOne)
            if (category?.ToUpper() != "AIRTIME" && category?.ToUpper() != "DATA")
            {
                var billersResponse = await _billerOneService.GetBillersAsync();
                billers = billersResponse?.Billers?
                    .Where(b => string.Equals(b.Category, category, StringComparison.OrdinalIgnoreCase))
                    .Select(b => new BillerItem
                    {
                        BillerId = b.BillerId,
                        BillerName = b.BillerName,
                        Description = b.Description,
                        LogoUrl = b.LogoPath,
                        ReferenceIdVerifiable = b.ReferenceIdVerifiable,
                        AmountInVerification = b.AmountInVerification
                    })
                    .ToList() ?? new List<BillerItem>();
            }

            var model = new PurchaseDetailsViewModel
            {
                Category = category ?? string.Empty,
                CategoryDisplayName = GetCategoryDisplayName(category ?? string.Empty),
                CategoryIcon = GetCategoryIcon(category ?? string.Empty),
                Billers = billers,
                CustomerName = $"{user.FirstName} {user.LastName}"
            };

            return View("~/Views/Home/Templates/PurchaseDetails.cshtml", model);
        }

        // ── SHARED: Review & Pay ─────────────────────────────────────────────────────
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ReviewAndPay(
            string category, string billerId, string billerName,
            string referenceNumber, string customerName,
            string planLabel, string planDuration,
            decimal amount, bool saveBeneficiary)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 0);

            var model = new ReviewAndPayViewModel
            {
                Category = category,
                CategoryDisplayName = GetCategoryDisplayName(category),
                CategoryIcon = GetCategoryIcon(category),
                BillerId = billerId,
                BillerName = billerName,
                ReferenceNumber = referenceNumber,
                ReferenceLabel = GetReferenceLabel(category, billerName),
                CustomerName = customerName,
                PlanLabel = planLabel,
                PlanDuration = planDuration,
                Amount = amount,
                WalletBalance = wallet?.Balance ?? 0m,
                SaveBeneficiary = saveBeneficiary,
                IsFourStep = IsFourStep(category)
            };

            return View("~/Views/Home/Templates/ReviewAndPay.cshtml", model);
        }

        // ── SHARED: Process Payment ──────────────────────────────────────────────────
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ReviewAndPayViewModel model)
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
                return RedirectToAction("ReviewAndPay", new
                {
                    category = model.Category,
                    billerId = model.BillerId,
                    billerName = model.BillerName,
                    referenceNumber = model.ReferenceNumber,
                    customerName = model.CustomerName,
                    planLabel = model.PlanLabel,
                    planDuration = model.PlanDuration,
                    amount = model.Amount,
                    saveBeneficiary = model.SaveBeneficiary
                });
            }

            // Debit the wallet
            var debitRequest = new DebitRequest
            {
                Amount = model.Amount,
                CustomerId = user.CustomerId ?? string.Empty,
                Description = $"Bill payment - {model.BillerName} - {model.Category}",
                TraceId = $"TIDL{Guid.NewGuid().ToString("N")[..8]}"
            };

            var debitResponse = await _walletService.DebitWalletAsync(debitRequest);

            if (debitResponse?.ResponseHeader?.ResponseCode != ResponseCode.Successful)
            {
                TempData["PaymentError"] = "Payment could not be processed at this time. Please try again.";
                return RedirectToAction("ReviewAndPay", new
                {
                    category = model.Category,
                    billerId = model.BillerId,
                    billerName = model.BillerName,
                    referenceNumber = model.ReferenceNumber,
                    customerName = model.CustomerName,
                    planLabel = model.PlanLabel,
                    planDuration = model.PlanDuration,
                    amount = model.Amount,
                    saveBeneficiary = model.SaveBeneficiary
                });
            }

            // Update local balance
            var localUser = _context.UserAccounts.FirstOrDefault(u => u.CustomerId == user.CustomerId);
            if (localUser != null)
            {
                localUser.Balance = debitResponse.Balance;
                await _context.SaveChangesAsync();
            }
            if (model.SaveBeneficiary)
            {
                try
                {
                    var benefName = !string.IsNullOrEmpty(model.CustomerName)
                        ? model.CustomerName
                        : $"{model.BillerName} - {model.ReferenceNumber}";

                    await _billerOneService.CreateBeneficiaryAsync(new CreateBeneficiaryRequest
                    {
                        BenefName = benefName,
                        BenefRefId = model.ReferenceNumber,
                        Biller = model.BillerName,
                        Category = model.Category ?? string.Empty
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save beneficiary for {BillerName}", model.BillerName);
                    // Don't fail the payment if saving the beneficiary fails
                }
            }

            // TODO: Add BillerOne payment call here when Mitchel integrates the payment endpoint

            var rng = new Random();
            string? token = null;
            decimal unitValue = 0;

            if (model.Category?.ToUpper() == "ELECTRICITY")
            {
                token = string.Join(" ", Enumerable.Range(0, 5).Select(_ => rng.Next(1000, 9999).ToString()));
                unitValue = Math.Round(model.Amount / 86, 1);
            }

            var successDesc = model.Category?.ToUpper() switch
            {
                "ELECTRICITY" => $"Paid to {model.BillerName} Prepaid",
                "AIRTIME" => $"{model.BillerName} Airtime sent to {model.ReferenceNumber}",
                "DATA" => $"{model.PlanLabel} data sent to {model.ReferenceNumber}",
                "DIGITALTV" => $"{model.BillerName} — {model.PlanLabel}",
                "EDUCATION" => $"Paid to {model.BillerName}",
                "GAMES" => $"Deposited to {model.BillerName} — User ID: {model.ReferenceNumber}",
                "TRANSPORT" => $"Paid to {model.BillerName}",
                _ => $"Paid to {model.BillerName}"
            };

            var successModel = new PaymentSuccessViewModel
            {
                Category = model.Category ?? string.Empty,
                BillerName = model.BillerName,
                ReferenceNumber = model.ReferenceNumber,
                ReferenceLabel = GetReferenceLabel(model.Category ?? string.Empty, model.BillerName),
                PlanLabel = model.PlanLabel,
                PlanDuration = model.PlanDuration,
                SuccessDescription = successDesc,
                Amount = model.Amount,
                PaidAt = DateTime.Now,
                TransactionRef = $"LP-{rng.Next(1000000, 9999999)}-X",
                PointsEarned = (int)(model.Amount * 0.02m),
                IsFourStep = model.IsFourStep,
                ElectricityToken = token,
                UnitValue = unitValue
            };

            return View("~/Views/Home/Templates/PaymentSuccess.cshtml", successModel);
        }

        // ── OTHER UTILITIES ──────────────────────────────────────────────────────────
        [Authorize]
        public IActionResult OtherUtilities()
        {
            return View("~/Views/Home/Templates/OtherUtilities.cshtml");
        }
    }
}