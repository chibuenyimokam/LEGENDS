using LegendPay.Models.ViewModels;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminSettlementService
    {
        Task<AdminSettlementViewModel> GetSettlementAsync();
    }
}
