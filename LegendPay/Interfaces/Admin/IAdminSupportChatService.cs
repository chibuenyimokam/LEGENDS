using LegendPay.Models.Data.Tables;
using LegendPay.Services;

namespace LegendPay.Interfaces.Admin
{
    public interface IAdminSupportChatService
    {
        Task<ServiceResponse<List<SupportChat>>> GetAllChatsAsync(string? statusFilter = null);
        Task<ServiceResponse<SupportChat>> GetChatAsync(Guid chatId);
        Task<ServiceResponse<SupportMessage>> SendReplyAsync(Guid chatId, Guid adminAccountId, string messageText);
        Task<ServiceResponse<SupportChat>> UpdateChatStatusAsync(Guid chatId, string newStatus);
        Task<int> GetAwaitingReplyCountAsync();
        Task<List<SupportChat>> GetAwaitingReplyChatsAsync();
    }
}