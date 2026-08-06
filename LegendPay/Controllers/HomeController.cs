using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.ViewModels.UserDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LegendPay.Controllers
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IBillPaymentHandler _paymentHandler;

        public HomeController(IAuthService authService, IBillPaymentHandler paymentHandler)
        {
            _authService = authService;
            _paymentHandler = paymentHandler;
        }

        public IActionResult Index() => View();
        public IActionResult Privacy() => View();

        [AllowAnonymous]
        public IActionResult Onboarding() => View();

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

            var model = await _paymentHandler.PreparePayBillsViewModelAsync(userId.Value);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> SelectBiller(string category)
        {
            var model = await _paymentHandler.PrepareSelectBillerViewModelAsync(category);
            return View("~/Views/Home/Templates/SelectBiller.cshtml", model);
        }

        [Authorize]
        public async Task<IActionResult> BillerDetails(
            string category, string billerId, string billerName,
            bool referenceIdVerifiable, bool amountInVerification)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var model = await _paymentHandler.PrepareBillerDetailsViewModelAsync(
                userEmail, category, billerId, billerName, referenceIdVerifiable, amountInVerification);

            if (model == null) return RedirectToAction("Login", "Auth");

            return View("~/Views/Home/Templates/BillerDetails.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ReviewAndPayViewModel model)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var result = await _paymentHandler.ProcessBillPaymentAsync(userEmail, model);

            if (!result.IsSuccess)
            {
                TempData["PaymentError"] = result.ErrorMessage;
                return RedirectToAction("ReviewAndPay", new
                {
                    category = model.Category,
                    billerId = model.BillerId,
                    billerName = model.BillerName,
                    packageSlug = model.PackageSlug,
                    referenceNumber = model.ReferenceNumber,
                    customerName = model.CustomerName,
                    planLabel = model.PlanLabel,
                    planDuration = model.PlanDuration,
                    amount = model.Amount,
                    saveBeneficiary = model.SaveBeneficiary
                });
            }

            return View("~/Views/Home/Templates/PaymentSuccess.cshtml", result.SuccessViewModel);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ReviewAndPay(
        string category, string billerId, string billerName, string packageSlug,
        string referenceNumber, string customerName,
        string planLabel, string planDuration,
        decimal amount, bool saveBeneficiary)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var model = await _paymentHandler.PrepareReviewAndPayViewModelAsync(
                userEmail, category, billerId, billerName, packageSlug,
                referenceNumber, customerName, planLabel,
                planDuration, amount, saveBeneficiary);

            if (model == null) return RedirectToAction("Login", "Auth");

            return View("~/Views/Home/Templates/ReviewAndPay.cshtml", model);
        }

        // Called via fetch() from PurchaseDetails.cshtml once a provider is selected,
        // so Airtime/Data can show real packages from coralpay instead of hardcoded values.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPackagesForBiller(int billerId)
        {
            var packages = await _paymentHandler.GetBillerPackagesAsync(billerId);
            return Json(packages.Select(p => new
            {
                slug = p.BillerItemId,
                label = p.Label,
                amount = p.Amount
            }));
        }

        [Authorize]
        public async Task<IActionResult> PurchaseDetails(string category, string mode = "")
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var model = await _paymentHandler.PreparePurchaseDetailsViewModelAsync(userEmail, category, mode);
            if (model == null) return RedirectToAction("Login", "Auth");

            return View("~/Views/Home/Templates/PurchaseDetails.cshtml", model);
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