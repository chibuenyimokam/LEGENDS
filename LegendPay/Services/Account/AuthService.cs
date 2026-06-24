using LegendPay.Interfaces.Auth;
using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.Data;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using LegendPay.Models.ViewModels.UserDashboard;
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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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

        public async Task<Wallet?> GetWalletWithRecentTransactionsAsync(Guid userId, int recentCount = 10) =>
            await _context.Wallets
                .AsNoTracking()
                .Include(w => w.WalletTransactions!
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(recentCount))
                .FirstOrDefaultAsync(w => w.UserAccountId == userId);

        public async Task<UserDashboardViewModel> GetUserDashboardAsync(UserAccount user)
        {
            var userId = user.Id;
            var now = DateTime.UtcNow;

            var wallet = await _context.Wallets.AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserAccountId == userId);

            var legendPoint = await _context.LegendPoints.AsNoTracking()
                .FirstOrDefaultAsync(lp => lp.UserAccountId == userId);

            var pendingBills = await _context.Bills.AsNoTracking()
                .Where(b => b.UserAccountId == userId && b.Status == "Pending")
                .OrderBy(b => b.CreatedAt)
                .ToListAsync();

            var spending = await _context.SpendingRecords.AsNoTracking()
                .Where(s => s.UserAccountId == userId && s.Year == now.Year && s.Month == now.Month)
                .ToListAsync();

            var renewals = await _context.Subscriptions.AsNoTracking()
                .Where(s => s.UserAccountId == userId && s.Status == "Active")
                .OrderBy(s => s.NextDueDate)
                .Take(6)
                .ToListAsync();

            var totalSpending = spending.Sum(s => s.TotalSpent);

            return new UserDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                WalletBalance = wallet?.Balance ?? 0m,
                LegendPoints = legendPoint == null ? 0 : legendPoint.TotalPoints - legendPoint.RedeemedPoints,
                PendingBillsCount = pendingBills.Count,
                PendingBillsTotal = pendingBills.Sum(b => b.Amount),
                TotalSpending = totalSpending,
                UpcomingBills = pendingBills
                    .Take(5)
                    .Select(b => new DashboardBillViewModel
                    {
                        Id = b.Id,
                        BillerName = b.BillerName,
                        Nickname = b.AccountReference,
                        Amount = b.Amount,
                        Status = b.Status
                    })
                    .ToList(),
                SpendingBreakdown = spending
                    .GroupBy(s => s.BillerCategory)
                    .Select(g => new SpendingSliceViewModel
                    {
                        Category = g.Key,
                        Amount = g.Sum(x => x.TotalSpent),
                        Percentage = totalSpending > 0
                            ? (double)(g.Sum(x => x.TotalSpent) / totalSpending) * 100
                            : 0
                    })
                    .OrderByDescending(s => s.Amount)
                    .ToList(),
                UpcomingRenewals = renewals
                    .Select(s => new RenewalViewModel
                    {
                        Id = s.Id,
                        BillerName = s.BillerName,
                        NextDueDate = s.NextDueDate,
                        Amount = s.Amount,
                        IsAutoPayEnabled = s.IsAutoPayEnabled
                    })
                    .ToList()
            };
        }

        public async Task<SubscriptionsViewModel> GetSubscriptionsAsync(Guid userId)
        {
            var subscriptions = await _context.Subscriptions.AsNoTracking()
                .Where(s => s.UserAccountId == userId && s.Status == "Active")
                .OrderBy(s => s.NextDueDate)
                .ToListAsync();

            var items = subscriptions
                .Select(s => new SubscriptionItemViewModel
                {
                    Id = s.Id,
                    BillerName = s.BillerName,
                    BillerCategory = s.BillerCategory,
                    Amount = s.Amount,
                    NextDueDate = s.NextDueDate,
                    RenewalIntervalDays = s.RenewalIntervalDays,
                    IsAutoPayEnabled = s.IsAutoPayEnabled,
                    Status = s.Status
                })
                .ToList();

            var totalMonthly = subscriptions.Sum(s =>
                s.RenewalIntervalDays <= 0 ? s.Amount : s.Amount * 30m / s.RenewalIntervalDays);

            DateTime? nextDue = subscriptions.Count > 0 ? subscriptions.Min(s => s.NextDueDate) : null;
            var overlap = nextDue.HasValue
                ? subscriptions.Count(s => s.NextDueDate.Date == nextDue.Value.Date)
                : 0;

            return new SubscriptionsViewModel
            {
                Subscriptions = items,
                TotalMonthlySpend = Math.Round(totalMonthly, 0),
                ActiveCount = subscriptions.Count,
                NextBillDue = nextDue,
                OverlapCount = overlap
            };
        }

        public async Task<BillHistoryViewModel> GetBillHistoryAsync(Guid userId, string? range, string? biller, string? amount, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var now = DateTime.UtcNow;
            var query = _context.Bills.AsNoTracking().Where(b => b.UserAccountId == userId);

            DateTime? from = range switch
            {
                "30d" => now.AddDays(-30),
                "3m" => now.AddMonths(-3),
                "ytd" => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                _ => null
            };
            if (from.HasValue)
                query = query.Where(b => b.CreatedAt >= from.Value);

            if (!string.IsNullOrWhiteSpace(biller) && biller != "all")
                query = query.Where(b => b.BillerName == biller);

            query = amount switch
            {
                "u5000" => query.Where(b => b.Amount <= 5000m),
                "5001-20000" => query.Where(b => b.Amount > 5000m && b.Amount <= 20000m),
                "20000p" => query.Where(b => b.Amount > 20000m),
                _ => query
            };

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BillHistoryItemViewModel
                {
                    Id = b.Id,
                    BillerName = b.BillerName,
                    BillerCategory = b.BillerCategory,
                    AccountReference = b.AccountReference,
                    Amount = b.Amount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            var billers = await _context.Bills.AsNoTracking()
                .Where(b => b.UserAccountId == userId)
                .Select(b => b.BillerName)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            return new BillHistoryViewModel
            {
                Transactions = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                Billers = billers,
                Range = range,
                Biller = biller,
                Amount = amount
            };
        }

        public async Task<ReceiptViewModel?> GetBillReceiptAsync(Guid billId, Guid userId)
        {
            var bill = await _context.Bills.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == billId && b.UserAccountId == userId);

            if (bill == null) return null;

            var reference = bill.ConfirmationToken
                ?? bill.BilleroneRefrence
                ?? bill.VergeRefrence
                ?? $"LP-{bill.Id.ToString("N")[..10].ToUpperInvariant()}";

            return new ReceiptViewModel
            {
                Id = bill.Id,
                BillerName = bill.BillerName,
                BillerCategory = bill.BillerCategory,
                AccountReference = bill.AccountReference,
                ReferenceId = reference,
                Amount = bill.Amount,
                Status = bill.Status,
                PaymentMethod = bill.PaymentMethod,
                CreatedAt = bill.CreatedAt
            };
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