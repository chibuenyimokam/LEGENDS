using LegendPay.Models.BillerOne.Response;

namespace LegendPay.Interfaces.Transaction
{
    public interface IBillerOneService
    {
        Task<GetCategoriesResponse?> GetCategoriesAsync(CancellationToken cancellationToken = default);
        Task<GetBillersResponse?> GetBillersAsync(CancellationToken cancellationToken = default);

    }
}
