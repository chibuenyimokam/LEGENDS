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
        private readonly IScheduledPaymentService _scheduledPaymentService;
        private readonly IWalletService _walletService;
        private readonly ILegendPointService _legendPointService;

        public HomeController(
                IAuthService authService,
                IBillPaymentHandler paymentHandler,
                IScheduledPaymentService scheduledPaymentService,
                IWalletService walletService,
                ILegendPointService legendPointService)
        {
            _authService = authService;
            _paymentHandler = paymentHandler;
            _walletService = walletService;
            _scheduledPaymentService = scheduledPaymentService;
            _legendPointService = legendPointService;
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
        public async Task<IActionResult> FundWallet(int Page = 1)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrEmpty(user.AccountNumber))
            {
                await _authService.TryProvisionWalletAsync(user);
            }
            const int itemsPerPage = 5;
            var historyResponse = await _walletService.GetTransactionHistoryAsync(
                user.CustomerId ?? string.Empty,
                page: Page,
                itemsPerPage: itemsPerPage);
            var transactions = historyResponse?.TransactionDetailsList?
                .Select(t => new RecentTransactionViewModel
                {
                    TransactionId = t.TransactionId,
                    Description = t.Description,
                    Amount = t.Amount,
                    Type = t.TranType, // "Credit" or "Debit"
                    Date = t.Date,
                    Status = "Successful"
                })
                .ToList() ?? new List<RecentTransactionViewModel>();

            var wallet = await _authService.GetWalletWithRecentTransactionsAsync(user.Id, 10);
            //var ledgerBalance = await _authService.GetLedgerBalanceAsync(user);

            var model = new WalletDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CustomerId = user.CustomerId ?? string.Empty,
                AccountNumber = wallet?.AccountNumber ?? user.AccountNumber ?? string.Empty,
                BankName = wallet?.BankName ?? user.BankName ?? string.Empty,
                WalletBalance = user.Balance,
                RecentTransactions = transactions,
                CurrentPage = historyResponse?.Pagination?.CurrentPage ?? Page,
                TotalPages = historyResponse?.Pagination?.TotalPages ?? 1

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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSubscription(string billerCategory, string billerName, string accountReference, decimal amount, int intervalDays)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var (success, message) = await _authService.CreateSubscriptionAsync(user.Id, billerCategory, billerName, accountReference, amount, intervalDays);
            TempData[success ? "SubSuccess" : "SubError"] = message;

            return RedirectToAction("Subscriptions");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CancelSubscription(Guid id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var success = await _authService.CancelSubscriptionAsync(id, user.Id);
            TempData[success ? "SubSuccess" : "SubError"] = success ? "Subscription cancelled." : "Could not cancel subscription.";

            return RedirectToAction("Subscriptions");
        }

        [Authorize]
        public async Task<IActionResult> ScheduledPayments()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _scheduledPaymentService.GetUserSchedulesAsync(user.Id);
            model.FirstName = user.FirstName;
            model.AvailableBalance = await _authService.GetLedgerBalanceAsync(user);
            model.Categories = (await _paymentHandler.PreparePayBillsViewModelAsync(user.Id)).Categories;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSchedule(CreateScheduledPaymentViewModel form)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
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
            model.Categories = (await _paymentHandler.PreparePayBillsViewModelAsync(user.Id)).Categories;
            model.Form = form;

            return View("ScheduledPayments", model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ScheduleBillers(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return Json(new List<object>());

            var vm = await _paymentHandler.PrepareSelectBillerViewModelAsync(category);
            var billers = vm.Billers.Select(b => new { name = b.BillerName }).ToList();
            return Json(billers);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CancelSchedule(Guid id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var (success, message) = await _scheduledPaymentService.CancelAsync(id, user.Id);
            TempData[success ? "ScheduleSuccess" : "ScheduleError"] = message;

            return RedirectToAction("ScheduledPayments");
        }

        [Authorize]
        public async Task<IActionResult> LegendPoints()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = await _legendPointService.GetUserPointsAsync(user.Id);
            model.FirstName = user.FirstName;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RedeemPoints(int points)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Auth");

            var (success, message) = await _legendPointService.RedeemAsync(user.Id, points);
            TempData[success ? "RedeemSuccess" : "RedeemError"] = message;

            return RedirectToAction("LegendPoints");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}