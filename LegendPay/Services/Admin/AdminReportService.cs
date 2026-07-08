using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminReportService : IAdminReportService
    {
        private readonly AppDbContext _context;

        public AdminReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminReportsViewModel> GetReportsAsync()
        {
            var totalTransactions = await _context.Bills.CountAsync();
            var successCount = await _context.Bills.CountAsync(b => b.Status == "Success");
            var totalRevenue = await _context.Bills
                .Where(b => b.Status == "Success")
                .SumAsync(b => (decimal?)b.Amount) ?? 0m;

            var avgValue = successCount > 0 ? totalRevenue / successCount : 0m;
            var successRate = totalTransactions > 0
                ? Math.Round((double)successCount / totalTransactions * 100, 1)
                : 0;

            var groups = await _context.Bills.AsNoTracking()
                .GroupBy(b => b.BillerName)
                .Select(g => new
                {
                    Name = g.Key,
                    Volume = g.Count(),
                    SuccessCount = g.Count(x => x.Status == "Success"),
                    Revenue = g.Sum(x => x.Status == "Success" ? x.Amount : 0m)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(8)
                .ToListAsync();

            var topBillers = groups.Select(x => new BillerPerformanceRow
            {
                Name = x.Name,
                Volume = x.Volume,
                Revenue = x.Revenue,
                SuccessRate = x.Volume > 0 ? Math.Round((double)x.SuccessCount / x.Volume * 100, 1) : 0
            }).ToList();

            return new AdminReportsViewModel
            {
                TotalRevenue = totalRevenue,
                TotalTransactions = totalTransactions,
                AvgTransactionValue = avgValue,
                SuccessRate = successRate,
                TopBillers = topBillers
            };
        }
    }
}
