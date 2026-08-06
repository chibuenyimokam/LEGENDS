using LegendPay.Models.ViewModels.UserDashboard;

namespace LegendPay.Interfaces.Transaction
{
    public interface IScheduledPaymentService
    {
        Task<ScheduledPaymentsViewModel> GetUserSchedulesAsync(Guid userId);
        Task<(bool Success, string Message)> CreateAsync(Guid userId, CreateScheduledPaymentViewModel model);
        Task<(bool Success, string Message)> CancelAsync(Guid scheduleId, Guid userId);
    }
}
