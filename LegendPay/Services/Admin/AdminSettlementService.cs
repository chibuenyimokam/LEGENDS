using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminSettlementService : IAdminSettlementService
    {
        private readonly AppDbContext _context;

        public AdminSettlementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminSettlementViewModel> GetSettlementAsync()
        {
            var totalFloat = await _context.UserAccounts.SumAsync(u => (decimal?)u.Balance) ?? 0m;
            var fundedAccounts = await _context.UserAccounts.CountAsync(u => u.AccountNumber != null);
            var totalCustomers = await _context.UserAccounts.CountAsync();

            var bankBreakdown = await _context.UserAccounts
                .Where(u => u.AccountNumber != null && u.BankName != null)
                .GroupBy(u => u.BankName!)
                .Select(g => new BankFloatItem
                {
                    BankName = g.Key,
                    Accounts = g.Count(),
                    Float = g.Sum(u => u.Balance)
                })
                .OrderByDescending(b => b.Float)
                .ToListAsync();

            var recent = await _context.WalletTransactions.AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new SettlementTransactionItem
                {
                    Reference = t.ExternalReference ?? t.Id.ToString(),
                    Type = t.Type,
                    Amount = t.Amount,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new AdminSettlementViewModel
            {
                TotalFloat = totalFloat,
                FundedAccounts = fundedAccounts,
                TotalCustomers = totalCustomers,
                BankBreakdown = bankBreakdown,
                RecentTransactions = recent
            };
        }
    }
}
