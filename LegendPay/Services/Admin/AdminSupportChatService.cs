using LegendPay.Enums;
using LegendPay.Interfaces.Admin;
using LegendPay.Models;
using LegendPay.Models.Data.Tables;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services.Admin
{
    public class AdminSupportChatService : IAdminSupportChatService
    {
        private readonly AppDbContext _context;

        public AdminSupportChatService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<SupportChat>>> GetAllChatsAsync(string? statusFilter = null)
        {
            try
            {
                var query = _context.SupportChats
                    .Include(c => c.UserAccount)
                    .Include(c => c.Messages)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(statusFilter))
                    query = query.Where(c => c.Status == statusFilter);

                var chats = await query
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();

                return ServiceResponse<List<SupportChat>>.SuccessResponse(chats, $"{chats.Count} chat(s) found.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<SupportChat>>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<SupportChat>> GetChatAsync(Guid chatId)
        {
            try
            {
                var chat = await _context.SupportChats
                    .Include(c => c.UserAccount)
                    .Include(c => c.Messages)
                    .Include(c => c.Bill)
                    .FirstOrDefaultAsync(c => c.Id == chatId);

                if (chat == null)
                    return ServiceResponse<SupportChat>.FailureResponse("Chat not found.");

                return ServiceResponse<SupportChat>.SuccessResponse(chat);
            }
            catch (Exception ex)
            {
                return ServiceResponse<SupportChat>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<SupportMessage>> SendReplyAsync(Guid chatId, Guid adminAccountId, string messageText)
        {
            try
            {
                var chat = await _context.SupportChats.FindAsync(chatId);
                if (chat == null)
                    return ServiceResponse<SupportMessage>.FailureResponse("Chat not found.");

                var message = new SupportMessage
                {
                    SupportChatId = chatId,
                    Sender = MessageSender.Admin.ToString(),
                    MessageText = messageText
                };

                _context.SupportMessages.Add(message);

                _context.Notifications.Add(new Notification
                {
                    UserAccountId = chat.UserAccountId,
                    Type = "SupportReply",
                    ReferenceId = chat.Id,
                    Message = $"Support replied to your ticket \"{chat.Subject}\". Tap to view the conversation."
                });

                chat.UpdatedAt = DateTime.UtcNow;
                if (chat.Status == SupportChatStatus.Open.ToString())
                    chat.Status = SupportChatStatus.InProgress.ToString();

                await _context.SaveChangesAsync();

                return ServiceResponse<SupportMessage>.SuccessResponse(message, "Reply sent.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<SupportMessage>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<int> GetAwaitingReplyCountAsync()
        {
            var openChats = await _context.SupportChats
                .Where(c => c.Status != "Closed" && c.Status != "Resolved")
                .Select(c => new
                {
                    LastSender = c.Messages!
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Sender)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return openChats.Count(c => c.LastSender == "User");
        }

        public async Task<List<SupportChat>> GetAwaitingReplyChatsAsync()
        {
            var chats = await _context.SupportChats
                .Include(c => c.UserAccount)
                .Include(c => c.Messages)
                .Where(c => c.Status != "Closed" && c.Status != "Resolved")
                .ToListAsync();

            return chats
                .Where(c => c.Messages != null && c.Messages.Count > 0
                    && c.Messages.OrderByDescending(m => m.CreatedAt).First().Sender == "User")
                .OrderByDescending(c => c.UpdatedAt)
                .ToList();
        }

        public async Task<ServiceResponse<SupportChat>> UpdateChatStatusAsync(Guid chatId, string newStatus)
        {
            try
            {
                if (!Enum.TryParse<SupportChatStatus>(newStatus, ignoreCase: true, out _))
                    return ServiceResponse<SupportChat>.FailureResponse(
                        $"'{newStatus}' is not a valid status. Use: Open, InProgress, Resolved, or Closed.");

                var chat = await _context.SupportChats.FindAsync(chatId);
                if (chat == null)
                    return ServiceResponse<SupportChat>.FailureResponse("Chat not found.");

                chat.Status = newStatus;
                chat.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ServiceResponse<SupportChat>.SuccessResponse(chat, $"Chat status updated to {newStatus}.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<SupportChat>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }
    }
}