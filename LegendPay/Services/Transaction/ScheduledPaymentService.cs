using LegendPay.Interfaces.Transaction;
using LegendPay.Models;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels.UserDashboard;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Transaction
{
    public class ScheduledPaymentService : IScheduledPaymentService
    {
        private readonly AppDbContext _context;

        public ScheduledPaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ScheduledPaymentsViewModel> GetUserSchedulesAsync(Guid userId)
        {
            var payments = await _context.ScheduledPayments.AsNoTracking()
                .Where(s => s.UserAccountId == userId)
                .OrderBy(s => s.Status == "Pending" ? 0 : 1)
                .ThenBy(s => s.ScheduledDate)
                .Select(s => new ScheduledPaymentItem
                {
                    Id = s.Id,
                    BillerName = s.BillerName,
                    BillerCategory = s.BillerCategory,
                    AccountReference = s.AccountReference,
                    Amount = s.Amount,
                    ScheduledDate = s.ScheduledDate,
                    Status = s.Status
                })
                .ToListAsync();

            return new ScheduledPaymentsViewModel
            {
                Payments = payments,
                Form = new CreateScheduledPaymentViewModel { ScheduledDate = DateTime.Today.AddDays(1) }
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(Guid userId, CreateScheduledPaymentViewModel model)
        {
            if (model.Amount <= 0)
                return (false, "Enter an amount greater than zero.");
            if (model.ScheduledDate.Date < DateTime.Today)
                return (false, "The scheduled date can't be in the past.");

            _context.ScheduledPayments.Add(new ScheduledPayment
            {
                UserAccountId = userId,
                BillerCategory = model.BillerCategory,
                BillerName = model.BillerName,
                AccountReference = model.AccountReference,
                Amount = model.Amount,
                ScheduledDate = model.ScheduledDate,
                PaymentMethod = "Wallet",
                Status = "Pending"
            });

            _context.Notifications.Add(new Notification
            {
                UserAccountId = userId,
                Type = "Reminder",
                Message = $"Payment of ₦{model.Amount:N2} to {model.BillerName} scheduled for {model.ScheduledDate:dd MMM yyyy}."
            });

            await _context.SaveChangesAsync();

            return (true, $"Payment to {model.BillerName} scheduled for {model.ScheduledDate:dd MMM yyyy}.");
        }

        public async Task<(bool Success, string Message)> CancelAsync(Guid scheduleId, Guid userId)
        {
            var schedule = await _context.ScheduledPayments
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.UserAccountId == userId);

            if (schedule == null)
                return (false, "Scheduled payment not found.");
            if (schedule.Status != "Pending")
                return (false, $"This payment can't be cancelled because it is already {schedule.Status.ToLower()}.");

            schedule.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return (true, "Scheduled payment cancelled.");
        }
    }
}
