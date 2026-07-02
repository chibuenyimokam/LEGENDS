namespace LegendPay.Models.ViewModels.UserDashboard
{
    public class ElectricityDetailsViewModel
    {
        public string BillerName { get; set; } = string.Empty;  // e.g. "IKEDC"
        public string BillerFullName { get; set; } = string.Empty;  // e.g. "Ikeja Electric"
        public string BillerLocation { get; set; } = string.Empty;  // e.g. "Lagos, Nigeria"
        public string CustomerName { get; set; } = string.Empty;  // logged-in user's full name
    }

    public class ElectricityReviewViewModel
    {
        public string BillerName { get; set; } = string.Empty;
        public string BillerFullName { get; set; } = string.Empty;
        public string BillerLocation { get; set; } = string.Empty;
        public string MeterNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal WalletBalance { get; set; }
        public bool SaveBeneficiary { get; set; }
    }

    public class ElectricitySuccessViewModel
    {
        public string BillerName { get; set; } = string.Empty;
        public string BillerFullName { get; set; } = string.Empty;
        public string MeterNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public string ElectricityToken { get; set; } = string.Empty;
        public decimal UnitValue { get; set; }
        public int PointsEarned { get; set; }
    }
}