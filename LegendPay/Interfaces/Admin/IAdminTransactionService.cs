using LegendPay.Models.ViewModels;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminTransactionService
    {
        Task<AdminTransactionsViewModel> GetTransactionsAsync(string? status, string? biller, string? method, int page, int pageSize);
    }
}
