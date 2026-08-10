using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using LegendPay.Models.ViewModels.UserDashboard;
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
        Task<UserAccount?> GetUserByIdAsync(Guid userId);
        Task<UserAccount?>CreateAndSaveUserAsync(SignUpViewModel model, string initialotp);
        Task<bool> GeneratePasswordResetAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);
        Task<UserAccount?> ValidateLoginCredentialsAsync(string identifier, string plainPassword);
        Task UpdateUserAsync(UserAccount user);
        Task<decimal?> GetUserBalanceAsync(string email);
        Task<decimal> GetLedgerBalanceAsync(UserAccount user);
        Task<bool> TryProvisionWalletAsync(UserAccount user);
        Task<UserAccount?> GetWalletWithRecentTransactionsAsync(Guid userId, int recentCount = 10);
        Task<UserDashboardViewModel> GetUserDashboardAsync(UserAccount user);
        Task<SubscriptionsViewModel> GetSubscriptionsAsync(Guid userId);
        Task<(bool Success, string Message)> CreateSubscriptionAsync(Guid userId, string billerCategory, string billerName, string accountReference, decimal amount, int intervalDays);
        Task<bool> CancelSubscriptionAsync(Guid subscriptionId, Guid userId);
        Task<BillHistoryViewModel> GetBillHistoryAsync(Guid userId, string? range, string? biller, string? amount, int page, int pageSize);
        Task<ReceiptViewModel?> GetBillReceiptAsync(Guid billId, Guid userId);
    }
}
