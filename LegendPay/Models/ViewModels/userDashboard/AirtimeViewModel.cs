namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class AirtimeDetailsViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
    }

    public class AirtimeReviewViewModel
    {
        public string Network { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal WalletBalance { get; set; }
        public bool SaveBeneficiary { get; set; }
    }

    public class AirtimeSuccessViewModel
    {
        public string Network { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
    }
}