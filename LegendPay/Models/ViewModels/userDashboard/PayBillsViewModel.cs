using LegendPay.Models.BillerOne.Response;

namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class PayBillsViewModel
    {
        public decimal AvailableBalance { get; set; }
        public List<RecentBillerViewModel> RecentFavorites { get; set; } = new();
        public List <BillerCategoryViewModel> Categories { get; set; }
        public List <BillerViewModel> Billers { get; set; }
    }

    public class BillerCategoryViewModel
    {
        public string Category { get; set; }
        public string? LogoUrl { get; set; }
        public string IconName { get; set; }
    }
    public class BillerViewModel
    {
        public string Category { get; set; }
        public string BillerName { get; set; }
        public string BillerId { get; set; }
        public string? LogoPath { get; set; }
        public string Description { get; set; }
        public bool AmountInVerification { get; set; }
        public bool ReferenceIdVerifiable { get; set; }
    }

    public class RecentBillerViewModel
    {
        public string BillerName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string LastPaidLabel { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
    }
}
