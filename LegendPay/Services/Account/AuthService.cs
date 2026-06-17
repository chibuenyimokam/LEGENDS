using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.Data;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using LegendPay.Models.WalletStation.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LegendPay.Services.Account
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IOtpService _otpService;
        private readonly IWalletService _walletService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            IOtpService otpService,
            IWalletService walletService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _otpService = otpService;
            _walletService = walletService;
            _logger = logger;
        }

        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string plainPassword, string hashedPassword) =>
            BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);

        public async Task<UserAccount?> GetUserByEmailAsync(string email) =>
            await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<UserAccount?> CreateAndSaveUserAsync(SignUpViewModel model, string initialOtp)
        {
            var user = new UserAccount
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = HashPassword(model.Password),
                PhoneNumber = model.PhoneNumber
            };

            _otpService.ConfigureUserOtp(user, initialOtp);
            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            // Attempt wallet creation — user is saved regardless of wallet outcome
            try
            {
                var walletRequest = new CreateWalletRequest
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CustomerAlias = user.Email   // email used as the unique customer alias
                    // BVN and Otp left null — add these if your CoralPay profile requires them
                };

                var wallet = await _walletService.CreateWalletAsync(walletRequest);

                if (wallet?.AccountDetails != null)
                {
                    user.CustomerId = wallet.AccountDetails.CustomerId;
                    user.AccountNumber = wallet.AccountDetails.AccountNumber;
                    user.BankName = wallet.AccountDetails.BankName;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning(
                        "Wallet creation returned null for {Email}. User registered without a wallet.",
                        user.Email);
                }
            }
            catch (Exception ex)
            {
                // Don't block registration — the user is already saved.
                // A background job or manual retry can create the wallet later.
                _logger.LogError(ex, "Error creating wallet for {Email}", user.Email);
            }

            return user;
        }

        public async Task<UserAccount?> ValidateLoginCredentialsAsync(string identifier, string plainPassword)
        {
            var user = await GetUserByEmailAsync(identifier);
            if (user != null && VerifyPassword(plainPassword, user.Password))
                return user;

            return null;
        }

        public async Task SignInUserAsync(HttpContext httpContext, UserAccount user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, "User")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        public async Task<decimal?> GetUserBalanceAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);

            if (user == null || string.IsNullOrEmpty(user.CustomerId))
                return null;

            return await _walletService.GetBalanceAsync(user.CustomerId);
        }

        public async Task SignOutUserAsync(HttpContext httpContext) =>
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}