using LegendPay.Services;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminEmailService
    {
        Task<ServiceResponse<string>> SendTwoFactorCodeAsync(string toEmail, string code);
    }
}