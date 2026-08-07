using LegendPay.Models.WalletStation.Request;
using LegendPay.Models.WalletStation.Response;

namespace LegendPay.Interfaces.Transaction
{
    public interface IWalletService
    {
        Task<CreateWalletResponse?> CreateWalletAsync(CreateWalletRequest walletRequest, CancellationToken cancellationToken = default);
        Task<CreditResponse?> CreditWalletAsync(CreditRequest creditRequest, CancellationToken cancellationToken = default);
        Task<DebitResponse?> DebitWalletAsync(DebitRequest debitRequest, CancellationToken cancellationToken = default);
        Task<DebitReversalResponse?> DebitReversalAsync(DebitReversalRequest debitReversalRequest, CancellationToken cancellationToken = default);
        Task<decimal?> GetBalanceAsync(string customerId, CancellationToken cancellationToken = default);
        Task<GetTransactionListResponse?> GetTransactionHistoryAsync(string customerId, int page = 1, int itemsPerPage = 10, CancellationToken cancellationToken = default);


    }
}