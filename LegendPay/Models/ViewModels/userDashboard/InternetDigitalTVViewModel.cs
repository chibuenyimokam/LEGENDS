namespace LegendPay.Models.ViewModels.UserDashboard
{

    public class InternetDetailsViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
    }

    public class InternetReviewViewModel
    {
        public string Network { get; set; } = string.Empty;
        public string PlanLabel { get; set; } = string.Empty;
        public string PlanDuration { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal WalletBalance { get; set; }
        public bool SaveBeneficiary { get; set; }
    }

    public class InternetSuccessViewModel
    {
        public string Network { get; set; } = string.Empty;
        public string PlanLabel { get; set; } = string.Empty;
        public string PlanDuration { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
    }

    

    public class DigitalTVDetailsViewModel
    {
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderFullName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
    }

    public class DigitalTVReviewViewModel
    {
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderFullName { get; set; } = string.Empty;
        public string SmartcardNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PackageLabel { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal WalletBalance { get; set; }
        public bool SaveBeneficiary { get; set; }
    }

    public class DigitalTVSuccessViewModel
    {
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderFullName { get; set; } = string.Empty;
        public string SmartcardNumber { get; set; } = string.Empty;
        public string PackageLabel { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
    }
}