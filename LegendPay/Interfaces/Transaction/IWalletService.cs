using LegendPay.Models.WalletStation.Request;
using LegendPay.Models.WalletStation.Response;

namespace LegendPay.Interfaces.Transaction
{
    public interface IWalletService
    {
        Task<CreateWalletResponse?> CreateWalletAsync(CreateWalletRequest walletRequest, CancellationToken cancellationToken = default);
        Task<decimal?> GetBalanceAsync(string customerId, CancellationToken cancellationToken = default);
    }
}