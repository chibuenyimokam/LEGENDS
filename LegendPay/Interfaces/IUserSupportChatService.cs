using LegendPay.Models.Data.Tables;
using LegendPay.Services;
using Microsoft.AspNetCore.Http;

namespace LegendPay.Interfaces
{
    public interface IUserSupportChatService
    {
        Task<ServiceResponse<SupportChat>> CreateChatAsync(Guid userAccountId, string subject, Guid? billId);
        Task<ServiceResponse<SupportMessage>> SendMessageAsync(Guid chatId, string sender, string messageText, IFormFile? attachment);
        Task<ServiceResponse<SupportChat>> GetChatAsync(Guid chatId);
        Task<ServiceResponse<List<SupportChat>>> GetUserChatsAsync(Guid userAccountId);
    }
}