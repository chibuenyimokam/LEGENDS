using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using System.Globalization;

namespace LegendPay.Interfaces.Auth
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string plainPassword, string hashedPassword);
        Task SignInUserAsync(HttpContext httpContext, UserAccount user);
        Task SignOutUserAsync(HttpContext httpContext);

        Task<UserAccount?> GetUserByEmailAsync(string email);
        Task<UserAccount?>CreateAndSaveUserAsync(SignUpViewModel model, string initialotp);
        Task<UserAccount?> ValidateLoginCredentialsAsync(string identifier, string plainPassword);
        Task<decimal?> GetUserBalanceAsync(string email);
    }
}
