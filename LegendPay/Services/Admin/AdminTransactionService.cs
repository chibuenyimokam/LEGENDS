using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminTransactionService : IAdminTransactionService
    {
        private readonly AppDbContext _context;

        public AdminTransactionService(AppDbContext context)
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

        public async Task<AdminTransactionsViewModel> GetTransactionsAsync(string? status, string? biller, string? method, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 15;

            var today = GetTodayStartUtc();
            var todayVolume = await _context.Bills
                .Where(b => b.CreatedAt >= today && b.Status == "Success")
                .SumAsync(b => (decimal?)b.Amount) ?? 0m;

            var todayTotal = await _context.Bills.CountAsync(b => b.CreatedAt >= today);
            var todaySuccess = await _context.Bills.CountAsync(b => b.CreatedAt >= today && b.Status == "Success");
            var successRate = todayTotal > 0 ? Math.Round((double)todaySuccess / todayTotal * 100, 1) : 0;

            var query = _context.Bills.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
                query = query.Where(b => b.Status == status);

            if (!string.IsNullOrWhiteSpace(biller) && biller != "all")
                query = query.Where(b => b.BillerName == biller);

            if (!string.IsNullOrWhiteSpace(method) && method != "all")
                query = query.Where(b => b.PaymentMethod == method);

            var total = await query.CountAsync();

            var rows = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new AdminTxnRegistryRow
                {
                    Id = b.Id,
                    UserName = $"{b.UserAccount.FirstName} {b.UserAccount.LastName}",
                    BillerName = b.BillerName,
                    Amount = b.Amount,
                    PaymentMethod = b.PaymentMethod,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            var billers = await _context.Bills.AsNoTracking()
                .Select(b => b.BillerName)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            return new AdminTransactionsViewModel
            {
                TodayVolume = todayVolume,
                SuccessRate = successRate,
                Transactions = rows,
                Billers = billers,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                Status = status,
                Biller = biller,
                Method = method
            };
        }
    }
}