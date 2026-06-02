using LegendPay.Models.Data.Tables;

namespace LegendPay.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp();
        void ConfigureUserOtp(UserAccount account, string otp);
        Task<bool> ValidateUserOtpAsync(string email, string enteredOtp);

    }
}
