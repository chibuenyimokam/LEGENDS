using LegendPay.Interfaces;
using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.ViewModels;
using LegendPay.Services;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly AppDbContext _context;
        private readonly IAdminEmailService _emailService;
        private readonly JwtService _jwtService;

        public AdminAuthService(AppDbContext context, IAdminEmailService emailService, JwtService jwtService)
        {
            _context = context;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        public async Task<ServiceResponse<string>> LoginAsync(AdminLoginViewModel model)
        {
            try
            {
                var admin = await _context.AdminAccounts
                    .FirstOrDefaultAsync(a => a.Email == model.Email);

                if (admin == null)
                    return ServiceResponse<string>.FailureResponse("Invalid email or password.");

                if (!admin.IsActive)
                    return ServiceResponse<string>.FailureResponse("Your account has been deactivated.");

                if (!BCrypt.Net.BCrypt.Verify(model.Password, admin.Password))
                    return ServiceResponse<string>.FailureResponse("Invalid email or password.");

                var twoFactorCode = new Random().Next(100000, 999999).ToString();
                admin.TwoFactorCode = twoFactorCode;
                admin.TwoFactorExpiration = DateTime.UtcNow.AddMinutes(10);
                await _context.SaveChangesAsync();

                await _emailService.SendTwoFactorCodeAsync(admin.Email, twoFactorCode);

                return ServiceResponse<string>.SuccessResponse(admin.Email, "2FA code sent to your email.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> VerifyTwoFactorAsync(string email, string twoFactorCode)
        {
            try
            {
                var admin = await _context.AdminAccounts
                    .FirstOrDefaultAsync(a => a.Email == email);

                if (admin == null)
                    return ServiceResponse<string>.FailureResponse("Admin not found.");

                if (admin.TwoFactorCode != twoFactorCode || admin.TwoFactorExpiration < DateTime.UtcNow)
                    return ServiceResponse<string>.FailureResponse("Invalid or expired 2FA code.");

                admin.TwoFactorCode = null;
                admin.TwoFactorExpiration = null;
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateAdminToken(admin);

                return ServiceResponse<string>.SuccessResponse(token, "Login successful.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> LogoutAsync()
        {
            try
            {
                return ServiceResponse<string>.SuccessResponse("", "Logged out successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }
    }
}