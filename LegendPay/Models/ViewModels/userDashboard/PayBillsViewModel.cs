namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class PayBillsViewModel
    {
        public decimal AvailableBalance { get; set; }
        public List<RecentBillerViewModel> RecentFavorites { get; set; } = new();
    }

    public class RecentBillerViewModel
    {
        public string BillerName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string LastPaidLabel { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
    }
}
