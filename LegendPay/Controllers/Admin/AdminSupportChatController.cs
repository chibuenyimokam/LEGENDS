using LegendPay.Enums;
using LegendPay.Hubs;
using LegendPay.Interfaces.Admin;
using LegendPay.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LegendPay.Controllers.Admin
{
    [Authorize]
    public class AdminSupportChatController : Controller
    {
        private readonly IAdminSupportChatService _adminSupportChatService;
        private readonly IHubContext<SupportChatHub> _hubContext;

        public AdminSupportChatController(
            IAdminSupportChatService adminSupportChatService,
            IHubContext<SupportChatHub> hubContext)
        {
            _adminSupportChatService = adminSupportChatService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> UserSupport(string? status = null)
        {
            var response = await _adminSupportChatService.GetAllChatsAsync(status);

            var viewModel = new AdminSupportInboxViewModel
            {
                Chats = response.Success ? response.Data : new(),
                ActiveStatusFilter = status,
                ErrorMessage = response.Success ? null : response.Message
            };

            return View("~/Views/Admin/SupportChat/UserSupport.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AdminSupport(Guid chatId)
        {
            var response = await _adminSupportChatService.GetChatAsync(chatId);

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return RedirectToAction("UserSupport");
            }

            var viewModel = new AdminSupportChatDetailViewModel
            {
                Chat = response.Data,
                Messages = response.Data.Messages?
                    .OrderBy(m => m.CreatedAt)
                    .ToList() ?? new()
            };

            return View("~/Views/Admin/SupportChat/AdminSupport.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendReply(Guid chatId, string replyText)
        {
            if (string.IsNullOrWhiteSpace(replyText))
            {
                TempData["ErrorMessage"] = "Reply cannot be empty.";
                return RedirectToAction("AdminSupport", new { chatId });
            }

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (adminIdClaim == null)
                return RedirectToAction("Login", "Admin");

            var adminId = Guid.Parse(adminIdClaim);

            var response = await _adminSupportChatService.SendReplyAsync(chatId, adminId, replyText);

            if (response.Success)
            {
                await _hubContext.Clients.Group(chatId.ToString())
                    .SendAsync(
                        "ReceiveMessage",
                        MessageSender.Admin.ToString(),
                        replyText,
                        response.Data.CreatedAt.ToString("hh:mm tt"));

                TempData["SuccessMessage"] = "Reply sent.";
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction("AdminSupport", new { chatId });
        }

        [HttpGet]
        public async Task<IActionResult> OpenCount()
        {
            var count = await _adminSupportChatService.GetAwaitingReplyCountAsync();
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> Awaiting()
        {
            var chats = await _adminSupportChatService.GetAwaitingReplyChatsAsync();
            var items = chats.Select(c => new
            {
                chatId = c.Id,
                subject = c.Subject,
                user = c.UserAccount != null ? $"{c.UserAccount.FirstName} {c.UserAccount.LastName}" : "Unknown"
            });
            return Json(new { items });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid chatId, string newStatus)
        {
            var response = await _adminSupportChatService.UpdateChatStatusAsync(chatId, newStatus);

            if (response.Success)
            {
                await _hubContext.Clients.Group(chatId.ToString())
                    .SendAsync("ChatStatusChanged", newStatus);

                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction("AdminSupport", new { chatId });
        }
    }
}