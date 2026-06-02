using LegendPay.Interfaces;
using LegendPay.Models.Data.Tables;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LegendPay.Services
{
    public class AuthService : IAuthService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); //
        }

        public bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword); //
        }

        public async Task SignInUserAsync(HttpContext httpContext, UserAccount user)
        {
            var claims = new List<Claim> //
            {
                new Claim(ClaimTypes.Name, user.Email), //
                new Claim(ClaimTypes.GivenName, user.FirstName), //
                new Claim(ClaimTypes.Surname, user.LastName), //
                new Claim(ClaimTypes.Role, "User") //
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); //
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)); //
        }

        public async Task SignOutUserAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //
        }
    }
}