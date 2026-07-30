using LegendPay.Models.VAS.Response;

namespace LegendPay.Services.Vas
{
    public interface IVasService
    {
        Task<VasApiResponse<List<BillerGroup>>> GetBillerGroupsAsync(CancellationToken ct = default);

        Task<VasApiResponse<List<Biller>>> GetBillersByGroupIdAsync(int billerGroupId, CancellationToken ct = default);

        Task<VasApiResponse<List<Biller>>> GetBillersByGroupSlugAsync(string billerGroupSlug, CancellationToken ct = default);

        Task<VasApiResponse<List<VasPackage>>> GetPackagesByBillerIdAsync(int billerId, CancellationToken ct = default);

        Task<VasApiResponse<List<VasPackage>>> GetPackagesByBillerSlugAsync(string billerSlug, CancellationToken ct = default);

        /// Must be called first in any vend flow. billerId is required separately from
        /// request.BillerSlug because the X-Signature formula uses billerId.
        
        Task<VasApiResponse<CustomerLookupResponseData>> CustomerLookupAsync(
            CustomerLookupRequest request, string billerId, CancellationToken ct = default);

       
        Task<VasApiResponse<VendValueResponseData>> VendValueAsync(
            VendValueRequest request, string billerId, CancellationToken ct = default);

        Task<VasApiResponse<VendTransactionResponseData>> GetTransactionByPaymentReferenceAsync(
            string paymentReference, CancellationToken ct = default);

        Task<VasApiResponse<VendTransactionResponseData>> GetTransactionByTransactionIdAsync(
            string transactionId, CancellationToken ct = default);
    }
}