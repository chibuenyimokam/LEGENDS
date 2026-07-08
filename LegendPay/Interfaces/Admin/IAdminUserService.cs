using LegendPay.Models.ViewModels;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminUserService
    {
        Task<AdminUserRegistryViewModel> GetUserRegistryAsync(string? search, string? status, decimal? minBalance, int page, int pageSize);
    }
}
