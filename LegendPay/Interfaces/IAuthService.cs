using LegendPay.Models.Data.Tables;

namespace LegendPay.Interfaces
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string plainPassword, string hashedPassword);
        Task SignInUserAsync(HttpContext httpContext, UserAccount user);
        Task SignOutUserAsync(HttpContext httpContext);
    }
}
