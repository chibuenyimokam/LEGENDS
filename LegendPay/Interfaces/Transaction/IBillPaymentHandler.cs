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

        // this is a shared payment execution method for scheduled payments, it will be used for "pay now" and the background payment worker(idk if that's working yet but this will be used for it)
        Task<PaymentResult> ExecuteScheduledPaymentAsync(Guid userId, string category, string billerName, string packageSlug, string accountReference, decimal amount);
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public PaymentSuccessViewModel? SuccessViewModel { get; set; }
        public Guid? BillId { get; set; }

    }
}