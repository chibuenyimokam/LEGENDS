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
        private readonly IBillPaymentHandler _billPaymentHandler;


        public ScheduledPaymentService(AppDbContext context, IBillPaymentHandler billPaymentHandler)
        {
            _context = context;
            _billPaymentHandler = billPaymentHandler;
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
                    PackageSlug = s.PackageSlug,
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
                BillerId = model.BillerId,
                BillerName = model.BillerName,
                PackageSlug = model.PackageSlug,
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
        public async Task<(bool Success, string Message)> ExecuteAsync(Guid scheduleId, Guid userId)
        {
            var schedule = await _context.ScheduledPayments
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.UserAccountId == userId);

            if (schedule == null)
                return (false, "Scheduled payment not found.");
            if (schedule.Status != "Pending")
                return (false, $"This payment can't be processed because it is already {schedule.Status.ToLower()}.");

            schedule.Status = "Processing";
            await _context.SaveChangesAsync();

            try
            {

                var result = await _billPaymentHandler.ExecuteScheduledPaymentAsync(
                    userId, schedule.BillerCategory, schedule.BillerName,
                    schedule.PackageSlug ?? string.Empty, schedule.AccountReference, schedule.Amount);

                schedule.Status = result.IsSuccess ? "Processed" : "Failed";
                schedule.BillId = result.BillId;

                _context.Notifications.Add(new Notification
                {
                    UserAccountId = userId,
                    Type = result.IsSuccess ? "Success" : "Alert",
                    Message = result.IsSuccess
                        ? $"Payment of ₦{schedule.Amount:N2} to {schedule.BillerName} was completed successfully."
                        : $"Scheduled payment of ₦{schedule.Amount:N2} to {schedule.BillerName} failed: {result.ErrorMessage}"
                });

                await _context.SaveChangesAsync();

                return result.IsSuccess
                    ? (true, $"Payment to {schedule.BillerName} was completed successfully.")
                    : (false, result.ErrorMessage ?? "Payment failed. Please try again.");
            }
            catch (Exception ex)
            {
                schedule.Status = "Pending";
                await _context.SaveChangesAsync();
                return (false, "An unexpected error occurred processing your payment.");
            }
            
        }

        
        private static DateTime GetTodayNigeria()
        {
            return (DateTime.UtcNow + TimeSpan.FromHours(1)).Date;
        }

        public async Task<int> ProcessDuePaymentsAsync()
        {
            var todayNigeria = GetTodayNigeria();

            
            var dueScheduleIds = await _context.ScheduledPayments.AsNoTracking()
                .Where(s => s.Status == "Pending" && s.ScheduledDate.Date <= todayNigeria)
                .Select(s => new { s.Id, s.UserAccountId })
                .ToListAsync();

            int processed = 0;
            foreach (var due in dueScheduleIds)
            {
                var (success, _) = await ExecuteAsync(due.Id, due.UserAccountId);
                if (success) processed++;
            }

            return processed;
        }
    }
}