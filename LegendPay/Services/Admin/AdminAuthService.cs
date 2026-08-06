using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using LegendPay.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LegendPay.Services.Admin
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly AppDbContext _context;
        private readonly IAdminEmailService _emailService;

        public AdminAuthService(AppDbContext context, IAdminEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        public async Task<ServiceResponse<string>> VerifyTwoFactorAsync(string email, string twoFactorCode, HttpContext httpContext)
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

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                    new Claim(ClaimTypes.Role, admin.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return ServiceResponse<string>.SuccessResponse("", "Login successful.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> ForgotPasswordAsync(string email)
        {
            try
            {
                var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.Email == email);

                if (admin != null && admin.IsActive)
                {
                    var code = new Random().Next(100000, 999999).ToString();
                    admin.TwoFactorCode = code;
                    admin.TwoFactorExpiration = DateTime.UtcNow.AddMinutes(10);
                    await _context.SaveChangesAsync();

                    await _emailService.SendTwoFactorCodeAsync(admin.Email, code);
                }

                return ServiceResponse<string>.SuccessResponse(email, "If an account exists for that email, a reset code has been sent.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<string>> ResetPasswordAsync(string email, string code, string newPassword)
        {
            try
            {
                var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.Email == email);

                if (admin == null || admin.TwoFactorCode != code || admin.TwoFactorExpiration == null || admin.TwoFactorExpiration < DateTime.UtcNow)
                    return ServiceResponse<string>.FailureResponse("Invalid or expired code. Please request a new one.");

                admin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                admin.TwoFactorCode = null;
                admin.TwoFactorExpiration = null;
                await _context.SaveChangesAsync();

                return ServiceResponse<string>.SuccessResponse("", "Your password has been reset. Please log in.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<AdminAccount?> GetAdminByIdAsync(Guid adminId) =>
            await _context.AdminAccounts.FirstOrDefaultAsync(a => a.Id == adminId);

        public async Task<ServiceResponse<string>> UpdateProfileAsync(Guid adminId, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return ServiceResponse<string>.FailureResponse("First and last name are required.");

            var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.Id == adminId);
            if (admin == null)
                return ServiceResponse<string>.FailureResponse("Admin not found.");

            admin.FirstName = firstName.Trim();
            admin.LastName = lastName.Trim();
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("", "Your profile has been updated.");
        }

        public async Task<ServiceResponse<string>> ChangePasswordAsync(Guid adminId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                return ServiceResponse<string>.FailureResponse("Your new password must be at least 8 characters.");

            var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.Id == adminId);
            if (admin == null)
                return ServiceResponse<string>.FailureResponse("Admin not found.");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, admin.Password))
                return ServiceResponse<string>.FailureResponse("Your current password is incorrect.");

            admin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return ServiceResponse<string>.SuccessResponse("", "Your password has been changed.");
        }

        public async Task<ServiceResponse<string>> LogoutAsync(HttpContext httpContext)
        {
            try
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return ServiceResponse<string>.SuccessResponse("", "Logged out successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }
    }
}