using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        
        private static readonly TimeSpan NigeriaOffset = TimeSpan.FromHours(1);

        private static DateTime GetTodayStartUtc()
        {
            var nowInNigeria = DateTime.UtcNow + NigeriaOffset;
            var todayStartNigeria = nowInNigeria.Date;
            return todayStartNigeria - NigeriaOffset;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var today = GetTodayStartUtc();

            var totalUsers = await _context.UserAccounts.CountAsync();
            var verifiedUsers = await _context.UserAccounts.CountAsync(u => u.IsEmailVerified);

            var transactionsToday = await _context.Bills.CountAsync(b => b.CreatedAt >= today);
            var totalValueToday = await _context.Bills
                .Where(b => b.CreatedAt >= today && b.Status == "Success")
                .SumAsync(b => (decimal?)b.Amount) ?? 0m;
            var failedToday = await _context.Bills.CountAsync(b => b.CreatedAt >= today && b.Status == "Failed");

            var recent = await _context.Bills.AsNoTracking()
                .OrderByDescending(b => b.CreatedAt)
                .Take(8)
                .Select(b => new
                {
                    b.Id,
                    b.UserAccount.FirstName,
                    b.UserAccount.LastName,
                    b.BillerName,
                    b.Amount,
                    b.Status,
                    b.CreatedAt
                })
                .ToListAsync();

            return new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                VerifiedUsers = verifiedUsers,
                TransactionsToday = transactionsToday,
                TotalValueToday = totalValueToday,
                FailedToday = failedToday,
                RecentTransactions = recent.Select(b => new AdminTransactionRow
                {
                    Reference = "TXN-" + b.Id.ToString("N")[..8].ToUpperInvariant(),
                    UserName = $"{b.FirstName} {b.LastName}".Trim(),
                    BillerName = b.BillerName,
                    Amount = b.Amount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                }).ToList()
            };
        }
    }
}