namespace LegendPay.Models.ViewModels.userDashboard
{
    public class LegendPointViewModel
    {
        public string FirstName { get; set; } = string.Empty;

        public int AvailablePoints { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }

        // Live values fetched from the admin-configured LegendPointSettings
        public decimal EarnRateBelowThousandPct { get; set; }
        public decimal EarnRateAboveThousandPct { get; set; }
        public decimal RedemptionRatePct { get; set; }
        public int MinimumRedemptionPoints { get; set; }

        // Derived
        public decimal CashValuePerPoint { get; set; }
        public decimal MaxCashValue { get; set; }
        public bool CanRedeem { get; set; }

        public List<LegendPointHistoryItem> History { get; set; } = new();
    }

    public class LegendPointHistoryItem
    {
        public string Type { get; set; } = string.Empty;
        public int Points { get; set; }
        public decimal? CashbackValue { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
