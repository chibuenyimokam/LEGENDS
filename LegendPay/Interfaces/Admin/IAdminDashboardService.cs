using LegendPay.Models.ViewModels;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
    }
}
