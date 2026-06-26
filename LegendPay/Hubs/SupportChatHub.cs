using Microsoft.AspNetCore.SignalR;

namespace LegendPay.Hubs
{
    public class SupportChatHub : Hub
    {
        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task SendMessage(string chatId, string sender, string messageText, string time)
        {
            await Clients.Group(chatId).SendAsync("ReceiveMessage", sender, messageText, time);
        }
    }
}