using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using LegendPay.Services;
using Microsoft.AspNetCore.Http;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminAuthService
    {
        Task<ServiceResponse<string>> LoginAsync(AdminLoginViewModel model);
        Task<ServiceResponse<string>> VerifyTwoFactorAsync(string email, string twoFactorCode, HttpContext httpContext);
        Task<ServiceResponse<string>> LogoutAsync(HttpContext httpContext);
        Task<ServiceResponse<string>> ForgotPasswordAsync(string email);
        Task<ServiceResponse<string>> ResetPasswordAsync(string email, string code, string newPassword);
        Task<AdminAccount?> GetAdminByIdAsync(Guid adminId);
        Task<ServiceResponse<string>> UpdateProfileAsync(Guid adminId, string firstName, string lastName);
        Task<ServiceResponse<string>> ChangePasswordAsync(Guid adminId, string currentPassword, string newPassword);
    }
}