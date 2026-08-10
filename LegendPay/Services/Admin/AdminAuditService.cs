using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminAuditService : IAdminAuditService
    {
        private readonly AppDbContext _context;

        public AdminAuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminAuditViewModel> GetAuditLogAsync()
        {
            var entries = new List<AuditEntry>();

            var registrations = await _context.UserAccounts.AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Take(30)
                .Select(u => new AuditEntry
                {
                    Actor = "System",
                    Action = "New customer registered",
                    Detail = u.Email,
                    Icon = "person_add",
                    Timestamp = u.CreatedAt
                })
                .ToListAsync();
            entries.AddRange(registrations);

            var adminReplies = await _context.SupportMessages.AsNoTracking()
                .Where(m => m.Sender == "Admin")
                .OrderByDescending(m => m.CreatedAt)
                .Take(30)
                .Select(m => new AuditEntry
                {
                    Actor = "Support admin",
                    Action = "Replied to a support ticket",
                    Detail = m.SupportChat.Subject,
                    Icon = "support_agent",
                    Timestamp = m.CreatedAt
                })
                .ToListAsync();
            entries.AddRange(adminReplies);

            var redemptions = await _context.LegendPointTransactions.AsNoTracking()
                .Where(t => t.Type == "Redeemed")
                .OrderByDescending(t => t.CreatedAt)
                .Take(30)
                .Select(t => new AuditEntry
                {
                    Actor = t.UserAccount.Email,
                    Action = "Redeemed Legend Points",
                    Detail = $"{t.Points:N0} points",
                    Icon = "redeem",
                    Timestamp = t.CreatedAt
                })
                .ToListAsync();
            entries.AddRange(redemptions);

            var settings = await _context.LegendPointSettings.AsNoTracking().FirstOrDefaultAsync();
            if (settings != null)
            {
                entries.Add(new AuditEntry
                {
                    Actor = "Admin",
                    Action = "Updated Legend Point settings",
                    Detail = $"Redemption rate {settings.RedemptionRate * 100m:0.##}%",
                    Icon = "stars",
                    Timestamp = settings.UpdatedAt
                });
            }

            return new AdminAuditViewModel
            {
                Entries = entries.OrderByDescending(e => e.Timestamp).Take(50).ToList()
            };
        }
    }
}
