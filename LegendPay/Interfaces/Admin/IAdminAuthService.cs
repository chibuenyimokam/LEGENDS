using LegendPay.Models.ViewModels.Auth;
using LegendPay.Services;
using Microsoft.AspNetCore.Http;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminAuthService
    {
        Task<ServiceResponse<string>> LoginAsync(AdminLoginViewModel model);
        Task<ServiceResponse<string>> VerifyTwoFactorAsync(string email, string twoFactorCode, HttpContext httpContext);
        Task<ServiceResponse<string>> LogoutAsync(HttpContext httpContext);
    }
}