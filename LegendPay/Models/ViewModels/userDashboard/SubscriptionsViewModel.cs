namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class SubscriptionsViewModel
    {
        public List<SubscriptionItemViewModel> Subscriptions { get; set; } = new();
        public decimal TotalMonthlySpend { get; set; }
        public int ActiveCount { get; set; }
        public DateTime? NextBillDue { get; set; }
        public int OverlapCount { get; set; }
    }

    public class SubscriptionItemViewModel
    {
        public Guid Id { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public string BillerCategory { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime NextDueDate { get; set; }
        public int RenewalIntervalDays { get; set; }
        public bool IsAutoPayEnabled { get; set; }
        public string Status { get; set; } = string.Empty;

        public string Badge => BillerBadge.From(BillerName);

        public string Cadence => RenewalIntervalDays switch
        {
            <= 0 => "Monthly",
            7 => "Weekly",
            14 => "Bi-weekly",
            30 or 31 => "Monthly",
            90 or 91 or 92 => "Quarterly",
            365 or 366 => "Yearly",
            _ => $"Every {RenewalIntervalDays} days"
        };
    }
}
