using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels.userDashboard;
using LegendPay.Models.WalletStation.Request;
using LegendPay.Models.WalletStation.Response;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Transaction
{
    public class LegendPointService : ILegendPointService
    {
        private readonly AppDbContext _context;
        private readonly IWalletService _walletService;
        private readonly ILogger<LegendPointService> _logger;

        public LegendPointService(AppDbContext context, IWalletService walletService, ILogger<LegendPointService> logger)
        {
            _context = context;
            _walletService = walletService;
            _logger = logger;
        }

        private async Task<LegendPointSettings> GetSettingsAsync() =>
            await _context.LegendPointSettings.AsNoTracking().FirstOrDefaultAsync() ?? new LegendPointSettings();

        private async Task<LegendPoint> GetOrCreateUserPointAsync(Guid userId)
        {
            var point = await _context.LegendPoints.FirstOrDefaultAsync(p => p.UserAccountId == userId);
            if (point == null)
            {
                point = new LegendPoint { UserAccountId = userId };
                _context.LegendPoints.Add(point);
                await _context.SaveChangesAsync();
            }
            return point;
        }

        public async Task<LegendPointViewModel> GetUserPointsAsync(Guid userId)
        {
            var settings = await GetSettingsAsync();
            var point = await GetOrCreateUserPointAsync(userId);
            var available = point.TotalPoints - point.RedeemedPoints;

            var history = await _context.LegendPointTransactions.AsNoTracking()
                .Where(t => t.UserAccountId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new LegendPointHistoryItem
                {
                    Type = t.Type,
                    Points = t.Points,
                    CashbackValue = t.CashbackValue,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new LegendPointViewModel
            {
                AvailablePoints = available,
                TotalEarned = point.TotalPoints,
                TotalRedeemed = point.RedeemedPoints,
                EarnRateBelowThousandPct = settings.EarnRateBelowThousand * 100m,
                EarnRateAboveThousandPct = settings.EarnRateAboveThousand * 100m,
                RedemptionRatePct = settings.RedemptionRate * 100m,
                MinimumRedemptionPoints = settings.MinimumRedemptionPoints,
                CashValuePerPoint = settings.RedemptionRate,
                MaxCashValue = available * settings.RedemptionRate,
                CanRedeem = available >= settings.MinimumRedemptionPoints,
                History = history
            };
        }

        public async Task<(bool Success, string Message)> RedeemAsync(Guid userId, int points)
        {
            var settings = await GetSettingsAsync();
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return (false, "Account not found.");

            var point = await GetOrCreateUserPointAsync(userId);
            var available = point.TotalPoints - point.RedeemedPoints;

            if (points <= 0)
                return (false, "Enter a valid number of points to redeem.");
            if (available < settings.MinimumRedemptionPoints)
                return (false, $"You need at least {settings.MinimumRedemptionPoints:N0} points to redeem.");
            if (points < settings.MinimumRedemptionPoints)
                return (false, $"The minimum you can redeem at once is {settings.MinimumRedemptionPoints:N0} points.");
            if (points > available)
                return (false, "You don't have that many points.");
            if (string.IsNullOrEmpty(user.CustomerId))
                return (false, "Your wallet isn't activated yet, so cashback can't be paid out.");

            var cashValue = points * settings.RedemptionRate;

            CreditResponse? creditResponse = null;
            try
            {
                creditResponse = await _walletService.CreditWalletAsync(new CreditRequest
                {
                    Amount = cashValue,
                    CustomerId = user.CustomerId,
                    Description = "Legend Points redemption",
                    TraceId = $"RDMP{Guid.NewGuid().ToString("N")[..8]}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Legend Points redemption credit call errored for {UserId} ({Points} pts).", userId, points);
            }

            if (creditResponse == null)
            {
                _logger.LogWarning("Legend Points redemption credit failed for {UserId} ({Points} pts).", userId, points);
                return (false, "Cashback couldn't be paid to your wallet right now. No points were deducted — please try again.");
            }

            point.RedeemedPoints += points;
            point.UpdatedAt = DateTime.UtcNow;

            _context.LegendPointTransactions.Add(new LegendPointTransaction
            {
                LegendPointId = point.Id,
                UserAccountId = userId,
                Type = "Redeemed",
                Points = points,
                CashbackValue = cashValue,
                Description = $"Redeemed {points:N0} points for ₦{cashValue:N2} cashback"
            });

            _context.Notifications.Add(new Notification
            {
                UserAccountId = userId,
                Type = "CashbackCredited",
                Message = $"You redeemed {points:N0} Legend Points for ₦{cashValue:N2} cashback."
            });

            await _context.SaveChangesAsync();

            return (true, $"₦{cashValue:N2} cashback has been added to your wallet.");
        }

        public async Task<int> AwardPointsAsync(Guid userId, decimal billAmount, Guid? billId = null)
        {
            if (billAmount <= 0)
                return 0;

            var settings = await GetSettingsAsync();
            var point = await GetOrCreateUserPointAsync(userId);

            var rate = billAmount < 1000m ? settings.EarnRateBelowThousand : settings.EarnRateAboveThousand;
            var earned = (int)Math.Floor(billAmount * rate);
            if (earned <= 0)
                return 0;

            point.TotalPoints += earned;
            point.UpdatedAt = DateTime.UtcNow;

            _context.LegendPointTransactions.Add(new LegendPointTransaction
            {
                LegendPointId = point.Id,
                UserAccountId = userId,
                BillId = billId,
                Type = "Earned",
                Points = earned,
                BillAmount = billAmount,
                Description = $"Earned {earned:N0} points on a ₦{billAmount:N2} payment"
            });

            await _context.SaveChangesAsync();

            return earned;
        }
    }
}
