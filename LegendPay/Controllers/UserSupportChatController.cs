using LegendPay.Enums;
using LegendPay.Helpers;
using LegendPay.Hubs;
using LegendPay.Interfaces;
using LegendPay.Models.Data.Tables;
using LegendPay.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LegendPay.Controllers
{
    public class UserSupportChatController : Controller
    {
        private readonly IUserSupportChatService _supportChatService;
        private readonly IHubContext<SupportChatHub> _hubContext;

        public UserSupportChatController(IUserSupportChatService supportChatService, IHubContext<SupportChatHub> hubContext)
        {
            _supportChatService = supportChatService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> UserSupport()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdClaim);

            var chatsResponse = await _supportChatService.GetUserChatsAsync(userId);

            var viewModel = new UserSupportChatViewModel
            {
                UserName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "User",
                AllChats = chatsResponse.Success ? chatsResponse.Data : new List<SupportChat>()
            };

            return View("~/Views/UserSupportChat/UserSupport.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> OpenChat(Guid chatId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdClaim);

            var chatResponse = await _supportChatService.GetChatAsync(chatId);

            if (!chatResponse.Success)
            {
                TempData["ErrorMessage"] = chatResponse.Message;
                return RedirectToAction("UserSupport");
            }

            // loads ALL chats for the sidebar
            var allChatsResponse = await _supportChatService.GetUserChatsAsync(userId);

            var viewModel = new UserSupportChatViewModel
            {
                ChatId = chatResponse.Data.Id,
                UserName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "User",
                Messages = chatResponse.Data.Messages?
                    .OrderBy(m => m.CreatedAt)
                    .ToList() ?? new List<SupportMessage>(),
                ActiveChat = chatResponse.Data,
                AllChats = allChatsResponse.Success ? allChatsResponse.Data : new List<SupportChat>()
            };

            return View("~/Views/UserSupportChat/UserSupport.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> NewDispute(string subject)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdClaim);

            var response = await _supportChatService.CreateChatAsync(userId, subject, null);

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return RedirectToAction("UserSupport");
            }

            return RedirectToAction("OpenChat", new { chatId = response.Data.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(Guid chatId, string messageText, IFormFile? attachment)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return RedirectToAction("Login", "Auth");

            var response = await _supportChatService.SendMessageAsync(
                chatId,
                MessageSender.User.ToString(),
                messageText,
                attachment);

            if (response.Success)
            {
                await _hubContext.Clients.Group(chatId.ToString())
                    .SendAsync("ReceiveMessage",
                        MessageSender.User.ToString(),
                        messageText,
                        WatTime.FromUtc(response.Data.CreatedAt).ToString("hh:mm tt"));

                var chatResponse = await _supportChatService.GetChatAsync(chatId);
                if (chatResponse.Success && chatResponse.Data.Messages.Count == 3)
                {
                    var messages = chatResponse.Data.Messages.OrderBy(m => m.CreatedAt).ToList();

                    await _hubContext.Clients.Group(chatId.ToString())
                        .SendAsync("ReceiveMessage",
                            MessageSender.Admin.ToString(),
                            messages[1].MessageText,
                            WatTime.FromUtc(messages[1].CreatedAt).ToString("hh:mm tt"));

                    await _hubContext.Clients.Group(chatId.ToString())
                        .SendAsync("ReceiveMessage",
                            MessageSender.Admin.ToString(),
                            messages[2].MessageText,
                            WatTime.FromUtc(messages[2].CreatedAt).ToString("hh:mm tt"));
                }
            }

            return RedirectToAction("OpenChat", new { chatId });
        }
    }
}