using LegendPay.Interfaces.Auth;
using LegendPay.Models;
using LegendPay.Models.Data;
using LegendPay.Models.Data.Tables;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Account
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;
        public OtpService (AppDbContext context)
        {
            _context = context;
        }

        public string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public async Task ConfigureUserOtpAsync(UserAccount account, string otp)
        {
            account.OtpCode = otp;
            account.OtpExpiration = DateTime.Now.AddMinutes(10);
            account.IsEmailVerified = false;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateUserOtpAsync(string email, string enteredOtp)
        {
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.OtpCode != enteredOtp || user.OtpExpiration < DateTime.Now)
            {
                return false;
            }

            user.IsEmailVerified = true;
            user.OtpCode = null;
            user.OtpExpiration = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public bool IsOtpValid(UserAccount account, string enteredOtp)
        {
            return account.OtpCode == enteredOtp 
                && account.OtpExpiration.HasValue
                && account.OtpExpiration > DateTime.Now;
        }
    }
}
