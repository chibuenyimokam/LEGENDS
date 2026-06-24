using LegendPay.Interfaces;
using LegendPay.Models;
using LegendPay.Models.Data;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace LegendPay.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IOtpService _otpService;

        //injecting appdbcontext and otpservice here instead of accounts controller because
        //we want to keep the controller thin and delegate all auth related logic to this service
        public AuthService(AppDbContext context, IOtpService otpService)
        {
            _context = context;
            _otpService = otpService;
        }
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }

        public async Task<UserAccount?> GetUserByEmailAsync(string email)
        {
            return await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserAccount?>CreateAndSaveUserAsync(SignUpViewModel model, string initialOtp)
        {
            var user = new UserAccount
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = HashPassword(model.Password),
                PhoneNumber = model.PhoneNumber
            };

            _otpService.ConfigureUserOtp(user, initialOtp);
            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<UserAccount?>ValidateLoginCredentialsAsync(string identifier, string plainPassword)
        {
            var user = await GetUserByEmailAsync(identifier);
            if (user != null && VerifyPassword(plainPassword, user.Password))
            {
                return user;
            }
            return null;
        }

        public async Task SignInUserAsync(HttpContext httpContext, UserAccount user)
        {
            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email), 
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName), 
                new Claim(ClaimTypes.Role, "User") 
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); 
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)); 
        }

        public async Task SignOutUserAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}