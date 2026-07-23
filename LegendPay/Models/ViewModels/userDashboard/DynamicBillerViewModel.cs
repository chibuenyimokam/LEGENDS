namespace LegendPay.Models.ViewModels.UserDashboard
{
    // Shared lightweight biller item used across templates
    public class BillerItem
    {
        public string BillerId { get; set; } = string.Empty;
        public string BillerName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool ReferenceIdVerifiable { get; set; }
        public bool AmountInVerification { get; set; }
    }

    // Template A — Step 1
    public class SelectBillerViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string CategoryDisplayName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public List<BillerItem> Billers { get; set; } = new();
    }

    // Template A — Step 2
    public class BillerDetailsViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string CategoryDisplayName { get; set; } = string.Empty;
        public string BillerId { get; set; } = string.Empty;
        public string BillerName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string BillerLocation { get; set; } = string.Empty;
        public bool ReferenceIdVerifiable { get; set; }
        public bool AmountInVerification { get; set; }
        public List<BillerPackageItem> Packages { get; set; } = new();   
    }

  
    public class BillerPackageItem
    {
        public string Label { get; set; } = string.Empty;
        public string BillerItemId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    // Template B — Step 1
    public class PurchaseDetailsViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string CategoryDisplayName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public List<BillerItem> Billers { get; set; } = new();
        public string CustomerName { get; set; } = string.Empty;
    }

    // Shared — Review & Pay
    public class ReviewAndPayViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string CategoryDisplayName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public string BillerId { get; set; } = string.Empty;
        public string BillerName { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string ReferenceLabel { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PlanLabel { get; set; } = string.Empty;
        public string PlanDuration { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal WalletBalance { get; set; }
        public bool SaveBeneficiary { get; set; }
        public bool IsFourStep { get; set; }
    }

    // Shared — Payment Success
    public class PaymentSuccessViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string BillerName { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string ReferenceLabel { get; set; } = string.Empty;
        public string PlanLabel { get; set; } = string.Empty;
        public string PlanDuration { get; set; } = string.Empty;
        public string SuccessDescription { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
        public bool IsFourStep { get; set; }
        // Electricity only
        public string? ElectricityToken { get; set; }
        public decimal UnitValue { get; set; }
    }
}