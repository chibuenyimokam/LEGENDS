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
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Auth");

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null) return RedirectToAction("Login", "Auth");

            //If the wallet background creation originally failed, retry
            if (string.IsNullOrEmpty(user.CustomerId)) // should be customerId
            {
                bool provisionSuccess = await _authService.TryProvisionWalletAsync(user);
                if (!provisionSuccess)
                {
                    var pendingVm = new WalletDashboardViewModel
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Balance = 0,
                        AccountNumber = "Pending Activation",
                        CustomerId = "Pending Activation",
                        BankName = "Unavailable"
                    };

                    return View(pendingVm);
                }
            }

            var balance = await _walletService.GetBalanceAsync(user.CustomerId);
            if (balance.HasValue && balance.Value != user.Balance)
            {
                user.Balance = balance.Value;
                await _authService.UpdateUserAsync(user);
            }

            var vm = new WalletDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Balance = user.Balance, //balance.HasValue ? balance.Value.ToString("N2") : "0.00"
                AccountNumber = user.AccountNumber,
                BankName = user.BankName,
                CustomerId = user.CustomerId
            };

            return View("HomePage", vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
