using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
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
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Accounts");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Accounts");

            //If the wallet background creation originally failed, retry
            if (string.IsNullOrEmpty(user.CustomerId))
            {
                bool provisionSuccess = await _authService.TryProvisionWalletAsync(user);
                if (!provisionSuccess)
                {
                    ViewBag.FullName = $"{user.FirstName} {user.LastName}";
                    ViewBag.Balance = "Unavailable";
                    ViewBag.WalletId = "Pending Activation";
                    return View();
                }
            }

            var balance = await _authService.GetUserBalanceAsync(user.Email);

            ViewBag.FullName = $"{user.FirstName} {user.LastName}";
            ViewBag.Balance = balance.HasValue ? balance.Value.ToString("N2") : "0.00";
            ViewBag.WalletId = user.AccountNumber; 

            return View("HomePage");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
