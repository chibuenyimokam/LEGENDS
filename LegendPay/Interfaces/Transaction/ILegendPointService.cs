using LegendPay.Models.ViewModels.userDashboard;

namespace LegendPay.Interfaces.Transaction
{
    public interface ILegendPointService
    {
        Task<LegendPointViewModel> GetUserPointsAsync(Guid userId);
        Task<(bool Success, string Message)> RedeemAsync(Guid userId, int points);
        Task AwardPointsAsync(Guid userId, decimal billAmount, Guid? billId = null);
    }
}
