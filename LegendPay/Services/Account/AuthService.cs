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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                try
                {
                    var walletRequest = new CreateWalletRequest
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CustomerAlias = user.Email
                    };

                    _logger.LogInformation("Sending wallet request: {@WalletRequest}", walletRequest);
                    var wallet = await _walletService.CreateWalletAsync(walletRequest);
                    //_logger.LogInformation("Wallet response: {@WalletResponse}", wallet);

                    if (wallet?.AccountDetails != null)
                    {
                        user.CustomerId = wallet.AccountDetails.CustomerId;
                        user.AccountNumber = wallet.AccountDetails.AccountNumber;
                        user.BankName = wallet.AccountDetails.BankName;

                        _context.UserAccounts.Update(user);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Wallet details added to transaction for user: {Email}", user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Wallet Engine returned empty payload for {Email}. Proceeding with account creation only.", user.Email);
                    }
                }
                catch (Exception walletEx)
                {
                    _logger.LogError(walletEx, "Upstream wallet provider failed during signup for {Email}. User account will be created without wallet identifiers.", user.Email);
                }

                await transaction.CommitAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical database failure registering user account for {Email}", model.Email);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> TryProvisionWalletAsync(UserAccount user)
        {
            if (!string.IsNullOrEmpty(user.CustomerId))
            {
                return true;
            }

            try
            {
                var walletRequest = new CreateWalletRequest
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CustomerAlias = user.Email
                };

                var wallet = await _walletService.CreateWalletAsync(walletRequest);

                if (wallet?.AccountDetails != null)
                {
                    user.CustomerId = wallet.AccountDetails.CustomerId;
                    user.AccountNumber = wallet.AccountDetails.AccountNumber;
                    user.BankName = wallet.AccountDetails.BankName;

                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Wallet successfully provisioned for user: {Email}", user.Email);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "External upstream provider failed during wallet creation for {Email}.", user.Email);
                return false;
            }
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
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || string.IsNullOrEmpty(user.CustomerId))
                return null;

            var externalBalance = await _walletService.GetBalanceAsync(user.CustomerId);
            return externalBalance;
        }

        public async Task UpdateUserAsync(UserAccount user)
        {
            _context.UserAccounts.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task SignOutUserAsync(HttpContext httpContext) =>
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}