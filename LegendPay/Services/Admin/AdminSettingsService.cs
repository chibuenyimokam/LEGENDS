using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminSettingsService : IAdminSettingsService
    {
        private readonly AppDbContext _context;

        public AdminSettingsService(AppDbContext context)
        {
            _context = context;
        }

        private async Task<LegendPointSettings> GetOrCreateSettingsAsync()
        {
            var settings = await _context.LegendPointSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new LegendPointSettings();
                _context.LegendPointSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public async Task<AdminLegendPointsViewModel> GetLegendPointsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();

            var totalIssued = await _context.LegendPoints.SumAsync(p => (long?)p.TotalPoints) ?? 0;
            var totalRedeemed = await _context.LegendPoints.SumAsync(p => (long?)p.RedeemedPoints) ?? 0;
            var activeUsers = await _context.LegendPoints.CountAsync(p => p.TotalPoints - p.RedeemedPoints > 0);

            return new AdminLegendPointsViewModel
            {
                EarnRateBelowThousandPct = settings.EarnRateBelowThousand * 100m,
                EarnRateAboveThousandPct = settings.EarnRateAboveThousand * 100m,
                RedemptionRatePct = settings.RedemptionRate * 100m,
                MinimumRedemptionPoints = settings.MinimumRedemptionPoints,
                UpdatedAt = settings.UpdatedAt,
                TotalPointsIssued = totalIssued,
                TotalRedeemedPoints = totalRedeemed,
                ActiveUsers = activeUsers
            };
        }

        public async Task UpdateLegendPointsAsync(AdminLegendPointsViewModel model, Guid? adminId)
        {
            var settings = await GetOrCreateSettingsAsync();

            settings.EarnRateBelowThousand = model.EarnRateBelowThousandPct / 100m;
            settings.EarnRateAboveThousand = model.EarnRateAboveThousandPct / 100m;
            settings.RedemptionRate = model.RedemptionRatePct / 100m;
            settings.MinimumRedemptionPoints = model.MinimumRedemptionPoints;
            settings.LastUpdatedByAdminId = adminId;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
