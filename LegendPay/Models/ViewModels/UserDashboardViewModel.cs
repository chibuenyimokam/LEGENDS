namespace LegendPay.Models.ViewModels
{
    public class AccountInfoViewModel
    {
        public string UserName { get; set; }
        public decimal AvailableBalance { get; set; }
        public string WalletId { get; set; }
    }

    public class LegendPointsViewModel
    {
        public int CurrentPoints { get; set; }
        public int GoalPoints { get; set; }
        public decimal AmountToNextReward { get; set; }
    }

    public class RecentActivityViewModel
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
    }

    public class UpcomingRenewalViewModel
    {
        public string ServiceName { get; set; }
        public string Detail { get; set; }
        public decimal Amount { get; set; }
        public int DaysUntilDue { get; set; }
        public bool IsAutoPay { get; set; }
    }

    public class UserDashboardViewModel
    {
        public AccountInfoViewModel AccountInfo { get; set; }
        public LegendPointsViewModel LegendPoints { get; set; }
        public List<RecentActivityViewModel> RecentActivities { get; set; }
        public List<UpcomingRenewalViewModel> UpcomingRenewals { get; set; }
    }
}

