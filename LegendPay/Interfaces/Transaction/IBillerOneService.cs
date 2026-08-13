using LegendPay.Models.BillerOne.Request;
using LegendPay.Models.BillerOne.Response;

namespace LegendPay.Interfaces.Transaction
{
    public interface IBillerOneService
    {
        Task<GetCategoriesResponse?> GetCategoriesAsync(CancellationToken cancellationToken = default);
        Task<GetBillersResponse?> GetBillersAsync(CancellationToken cancellationToken = default);
        Task<GetBillerPackagesResponse?> GetBillerPackagesAsync(string billerId, CancellationToken cancellationToken = default);
        Task<GetBeneficiaryResponse?> GetBeneficiariesAsync(CancellationToken cancellationToken = default);
        Task<CreateBeneficiaryResponse?> CreateBeneficiaryAsync(CreateBeneficiaryRequest request, CancellationToken cancellationToken = default);
        Task<DeleteBeneficiaryResponse?> DeleteBeneficiaryAsync(string benefId, CancellationToken cancellationToken = default);
    }
}
