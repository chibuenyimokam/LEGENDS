using LegendPay.Models.BillerOne.Response;
using LegendPay.Models.Vas;
using LegendPay.Models.VAS.Request;
using LegendPay.Models.VAS.Response;

namespace LegendPay.Interfaces.Transaction
{
    public interface IVasService
    {
        Task<VasApiResponse<List<BillerGroupEnquiry>>> GetBillerGroupsAsync(CancellationToken ct = default);

        //Task<VasApiResponse<List<BillerGroupIdEnquiryResponse>>> GetBillersByGroupIdAsync(int billerGroupId, CancellationToken ct = default);

        Task<VasApiResponse<List<BillerGroupSlugEnquiryResponse>>> GetBillersByGroupSlugAsync(string billerGroupSlug, CancellationToken ct = default);

        Task<VasApiResponse<List<PackagesEnquiryResponse>>> GetPackagesByBillerIdAsync(int billerId, CancellationToken ct = default);

        Task<VasApiResponse<List<PackagesEnquirySlugResponse>>> GetPackagesByBillerSlugAsync(string billerSlug, CancellationToken ct = default);

        Task<VasApiResponse<CustomerEnquiryResponse>> CustomerLookupAsync(
            CustomerEnquiryRequest request, CancellationToken ct = default);

        
        Task<VasApiResponse<VendValueResponse>> VendValueAsync(
            VendValueRequest request, CancellationToken ct = default);

        Task<VasApiResponse<VendTransactionEnquiryResponse>> GetTransactionByPaymentReferenceAsync(
            string paymentReference, CancellationToken ct = default);

        Task<VasApiResponse<VendTransactionEnquiryResponse>> GetTransactionByTransactionIdAsync(
            string transactionId, CancellationToken ct = default);
    }
}