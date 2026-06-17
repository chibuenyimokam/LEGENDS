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
        // References to the database context and injected services
        private readonly IAuthService _authService;
        private readonly IWalletService _walletService;
        private readonly ILogger<HomeController> _logger;


        // Constructor dependency injection to get instances of all required services
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
        // Only authenticated users can access this page
        [Authorize]
        public async Task<IActionResult> HomePage()
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            var firstname = HttpContext.User.FindFirst(ClaimTypes.GivenName)?.Value; // get authenticated user's first name from claims
            var lastname = HttpContext.User.FindFirst(ClaimTypes.Surname)?.Value;    // get authenticated user's last name from claims

            ViewBag.FullName = $"{firstname} {lastname}"; // combine and pass full name to the view

            //fetch live babalnce from the database via AuthService and pass it to the view
            var balance = await _authService.GetUserBalanceAsync(email);
            ViewBag.Balance = balance.HasValue
                ? balance.Value.ToString("N2")
                : "Unavailable";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
