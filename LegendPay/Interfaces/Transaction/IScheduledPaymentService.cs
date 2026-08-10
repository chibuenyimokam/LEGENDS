using LegendPay.Models.ViewModels.UserDashboard;

namespace LegendPay.Interfaces.Transaction
{
    public interface IScheduledPaymentService
    {
        Task<ScheduledPaymentsViewModel> GetUserSchedulesAsync(Guid userId);
        Task<(bool Success, string Message)> CreateAsync(Guid userId, CreateScheduledPaymentViewModel model);
        Task<(bool Success, string Message)> CancelAsync(Guid scheduleId, Guid userId);

        // Runs a single pending schedule immediately, used by our "Pay Now" button.
        Task<(bool Success, string Message)> ExecuteAsync(Guid scheduleId, Guid userId);

        // Used by the background worker to process scheduled payments.
        // Returns how many were processed.
        Task<int> ProcessDuePaymentsAsync();
    }
}
