using GalleryCart.Areas.Chat.Models;
using GalleryCart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using static GalleryCart.Areas.Chat.Models.ChatModel;

namespace GalleryCart.Areas.Chat.Controllers
{
    [Area("Chat")]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;

        public ChatController(IUserRepository userRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        public async Task<IActionResult> Chat(Guid id)
        {
            var viewModel = new ChatModel();

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            viewModel.CurrentUser = await _userRepository.GetAsync(u => u.Id == userId.Value);
            if (viewModel.CurrentUser == null) return RedirectToAction("Login", "Account", new { area = "" });

            await LoadChatUsers(userId.Value, viewModel);

            if (id != Guid.Empty)
            {
                viewModel.CurrentReceiver = await _userRepository.GetAsync(u => u.Id == id);
                if (viewModel.CurrentReceiver == null)
                    return RedirectToAction("AllArtist", "Artists", new { area = "Customer" });

                await LoadChatHistory(userId.Value, id, viewModel);
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ChatHistoryAsync(Guid receiverId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { success = false, message = "User not authenticated" });

            try
            {
                var messages = await _chatRepository
                    .GetAllQueryable(c => (c.SenderId == userId.Value && c.ReceiverId == receiverId) ||
                                         (c.SenderId == receiverId && c.ReceiverId == userId.Value))
                    .Include(c => c.Sender)
                    .Include(c => c.Receiver)
                    .OrderBy(c => c.Timestamp)
                    .ToListAsync();

                var chatHistory = messages.Select(m => new
                {
                    chatId = m.ChatId,
                    senderId = m.SenderId,
                    senderName = m.Sender?.UserName ?? "Unknown",
                    senderAvatar = m.Sender?.UserAvatar,
                    receiverId = m.ReceiverId,
                    receiverName = m.Receiver?.UserName ?? "Unknown",
                    message = m.Message,
                    timestamp = m.Timestamp
                }).ToList();

                return Json(chatHistory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chat history: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Failed to load chat history" });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("extension_userId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private async Task LoadChatUsers(Guid currentUserId, ChatModel viewModel)
        {
            try
            {
                // Get all messages where the user is either sender or receiver
                var userMessages = await _chatRepository
                    .GetAllQueryable(c => c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                    .Include(c => c.Sender)
                    .Include(c => c.Receiver)
                    .ToListAsync();

                // Group by conversation partner and get the latest message for each
                var conversations = userMessages
                    .GroupBy(c => c.SenderId == currentUserId ? c.ReceiverId : c.SenderId)
                    .Select(g => new
                    {
                        ConversationPartnerId = g.Key,
                        LatestMessage = g.OrderByDescending(c => c.Timestamp).First(),
                        MessageCount = g.Count()
                    })
                    .ToList();

                viewModel.ChatUsers = new List<ChatUserDto>();

                foreach (var conv in conversations.OrderByDescending(c => c.LatestMessage.Timestamp))
                {
                    var partner = conv.LatestMessage.SenderId == currentUserId
                        ? conv.LatestMessage.Receiver
                        : conv.LatestMessage.Sender;

                    if (partner != null)
                    {
                        viewModel.ChatUsers.Add(new ChatUserDto
                        {
                            UserId = partner.Id,
                            UserName = partner.UserName ?? "Unknown",
                            UserAvatar = partner.UserAvatar,
                            LastMessage = conv.LatestMessage.Message.Length > 50
                                ? conv.LatestMessage.Message.Substring(0, 50) + "..."
                                : conv.LatestMessage.Message,
                            LastMessageTime = conv.LatestMessage.Timestamp,
                            IsArtist = partner.IsArtits,
                            MessageCount = conv.MessageCount
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chat users: {ex.Message}");
                viewModel.ChatUsers = new List<ChatUserDto>();
            }
        }
        private async Task LoadChatHistory(Guid currentUserId, Guid receiverId, ChatModel viewModel)
        {
            try
            {
                viewModel.ChatHistory = await _chatRepository
                     .GetAllQueryable(c => (c.SenderId == currentUserId && c.ReceiverId == receiverId) ||
                                          (c.SenderId == receiverId && c.ReceiverId == currentUserId))
                     .Include(c => c.Sender)
                     .Include(c => c.Receiver)
                     .OrderBy(c => c.Timestamp)
                     .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chat history: {ex.Message}");
                viewModel.ChatHistory = new List<GalleryCart.Models.Models.Chat>();
            }
        }
    }
}
