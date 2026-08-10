using LegendPay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LegendPay.Controllers
{
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

     
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Json(new { count = 0 });

            var userId = Guid.Parse(userIdClaim);

            var count = await _context.Notifications
                .CountAsync(n => n.UserAccountId == userId && !n.IsRead);

            return Json(new { count });
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Json(new { notifications = new List<object>() });

            var userId = Guid.Parse(userIdClaim);

            await PruneAsync(userId);

            var notifications = await _context.Notifications
                .Where(n => n.UserAccountId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    n.Id,
                    n.Type,
                    n.Message,
                    n.IsRead,
                    n.ReferenceId,
                    Time = n.CreatedAt.ToString("hh:mm tt"),
                    Date = n.CreatedAt.ToString("MMM dd")
                })
                .ToListAsync();

            return Json(new { notifications });
        }

        
        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Json(new { success = false });

            var userId = Guid.Parse(userIdClaim);

            var unread = await _context.Notifications
                .Where(n => n.UserAccountId == userId && !n.IsRead)
                .ToListAsync();

            unread.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        
        [HttpPost]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return Json(new { success = false });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private async Task PruneAsync(Guid userId)
        {
            var readCutoff = DateTime.UtcNow.AddHours(-48);
            var hardCutoff = DateTime.UtcNow.AddDays(-30);

            var stale = await _context.Notifications
                .Where(n => n.UserAccountId == userId
                    && ((n.IsRead && n.CreatedAt < readCutoff) || n.CreatedAt < hardCutoff))
                .ToListAsync();

            if (stale.Count > 0)
            {
                _context.Notifications.RemoveRange(stale);
                await _context.SaveChangesAsync();
            }
        }
    }
}