using LegendPay.Models.ViewModels.UserDashboard;

namespace LegendPay.Interfaces.Transaction
{
    public interface IBillPaymentHandler
    {
        Task<PayBillsViewModel> PreparePayBillsViewModelAsync(Guid userId);
        Task<SelectBillerViewModel> PrepareSelectBillerViewModelAsync(string category);
        Task<BillerDetailsViewModel?> PrepareBillerDetailsViewModelAsync(string email, string category, string billerId, string billerName, bool referenceIdVerifiable, bool amountInVerification);
        Task<ReviewAndPayViewModel?> PrepareReviewAndPayViewModelAsync(string email, string category, string billerId, string billerName, string packageSlug, string referenceNumber, string customerName, string planLabel, string planDuration, decimal amount, bool saveBeneficiary);
        Task<PurchaseDetailsViewModel?> PreparePurchaseDetailsViewModelAsync(string email, string category, string mode = "");
        Task<PaymentResult> ProcessBillPaymentAsync(string userEmail, ReviewAndPayViewModel model);
        Task<List<BillerPackageItem>> GetBillerPackagesAsync(int billerId);
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public PaymentSuccessViewModel? SuccessViewModel { get; set; }
    }
}