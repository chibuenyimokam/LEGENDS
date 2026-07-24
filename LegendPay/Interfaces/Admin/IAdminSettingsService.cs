using LegendPay.Models.ViewModels;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminSettingsService
    {
        Task<AdminLegendPointsViewModel> GetLegendPointsAsync();
        Task UpdateLegendPointsAsync(AdminLegendPointsViewModel model, Guid? adminId);
    }
}
