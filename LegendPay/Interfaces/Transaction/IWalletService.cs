using LegendPay.Models.WalletStation.Request;
using LegendPay.Models.WalletStation.Response;

namespace LegendPay.Interfaces.Transaction
{
    public interface IWalletService
    {
        Task<CreateWalletResponse?> CreateWalletAsync(CreateWalletRequest request);
        Task<decimal?> GetBalanceAsync(string customerId);
    }
}