using LegendPay.Models.ViewModels;
using LegendPay.Services;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminAuthService
    {
        Task<ServiceResponse<string>> LoginAsync(AdminLoginViewModel model);
        Task<ServiceResponse<string>> VerifyTwoFactorAsync(string email, string twoFactorCode);
        Task<ServiceResponse<string>> LogoutAsync();
    }
}