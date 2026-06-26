using LegendPay.Enums;
using LegendPay.Interfaces;
using LegendPay.Models;
using LegendPay.Models.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LegendPay.Services
{
    public class UserSupportChatService : IUserSupportChatService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserSupportChatService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<ServiceResponse<SupportChat>> CreateChatAsync(Guid userAccountId, string subject, Guid? billId)
        {
            try
            {
                var chat = new SupportChat
                {
                    UserAccountId = userAccountId,
                    Subject = subject,
                    BillId = billId,
                    Status = SupportChatStatus.Open.ToString()
                };

                _context.SupportChats.Add(chat);
                await _context.SaveChangesAsync();

                return ServiceResponse<SupportChat>.SuccessResponse(chat, "Chat created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<SupportChat>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<SupportMessage>> SendMessageAsync(Guid chatId, string sender, string messageText, IFormFile? attachment)
        {
            try
            {
                string? attachmentPath = null;

                if (attachment != null && attachment.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "support");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Guid.NewGuid()}_{attachment.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachment.CopyToAsync(stream);
                    }

                    attachmentPath = $"/uploads/support/{fileName}";
                }

                var message = new SupportMessage
                {
                    SupportChatId = chatId,
                    Sender = sender,
                    MessageText = messageText,
                    AttachmentPath = attachmentPath
                };

                _context.SupportMessages.Add(message);

                var chat = await _context.SupportChats.FindAsync(chatId);
                if (chat != null)
                {
                    chat.UpdatedAt = DateTime.UtcNow;
                    if (chat.Status == SupportChatStatus.Open.ToString())
                        chat.Status = SupportChatStatus.InProgress.ToString();
                }

                await _context.SaveChangesAsync();

                var messageCount = await _context.SupportMessages
                    .CountAsync(m => m.SupportChatId == chatId);

                if (messageCount == 1 && sender == MessageSender.User.ToString())
                {
                    var autoReply1 = new SupportMessage
                    {
                        SupportChatId = chatId,
                        Sender = MessageSender.Admin.ToString(),
                        MessageText = "Thank you for reaching out to LegendPay Support! Kindly state your issue and we will be happy to assist you."
                    };
                    _context.SupportMessages.Add(autoReply1);
                    await _context.SaveChangesAsync();

                    var autoReply2 = new SupportMessage
                    {
                        SupportChatId = chatId,
                        Sender = MessageSender.Admin.ToString(),
                        MessageText = "To help us resolve your issue faster, please provide your transaction reference number or attach any relevant screenshots/receipts. An admin will reach out to you shortly."
                    };
                    _context.SupportMessages.Add(autoReply2);
                    await _context.SaveChangesAsync();
                }

                return ServiceResponse<SupportMessage>.SuccessResponse(message, "Message sent.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<SupportMessage>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<SupportChat>> GetChatAsync(Guid chatId)
        {
            try
            {
                var chat = await _context.SupportChats
                    .Include(c => c.Messages)
                    .Include(c => c.UserAccount)
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

        public async Task<ServiceResponse<List<SupportChat>>> GetUserChatsAsync(Guid userAccountId)
        {
            try
            {
                var chats = await _context.SupportChats
                    .Include(c => c.Messages)
                    .Where(c => c.UserAccountId == userAccountId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();

                return ServiceResponse<List<SupportChat>>.SuccessResponse(chats);
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<SupportChat>>.FailureResponse($"An error occurred: {ex.Message}");
            }
        }
    }
}