using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GalleryCart.Areas.Chat.Pages.Chat
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        public ChatModel(IUserRepository userRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        public User? CurrentUser { get; set; }
        public List<User> ConnectedUsers { get; set; } = new List<User>();
        public User? CurrentReceiver { get; set; }
        public List<ChatUserDto> ChatUsers { get; set; } = new List<ChatUserDto>();
        public List<Models.Models.Chat> ChatHistory { get; set; } = new List<Models.Models.Chat>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentUser = await _userRepository.GetAsync(u => u.Id == userId.Value);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Load chat users (users you've chatted with)
            await LoadChatUsers(userId.Value);

            if (id != Guid.Empty)
            {
                CurrentReceiver = await _userRepository.GetAsync(u => u.Id == id);
                if (CurrentReceiver == null)
                {
                    return RedirectToPage("/Artists/AllArtist", new { area = "Customer" });
                }

                // Load chat history for the selected receiver
                await LoadChatHistory(userId.Value, id);
            }

            return Page();
        }

        private async Task LoadChatUsers(Guid currentUserId)
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

                ChatUsers = new List<ChatUserDto>();

                foreach (var conv in conversations.OrderByDescending(c => c.LatestMessage.Timestamp))
                {
                    var partner = conv.LatestMessage.SenderId == currentUserId
                        ? conv.LatestMessage.Receiver
                        : conv.LatestMessage.Sender;

                    if (partner != null)
                    {
                        ChatUsers.Add(new ChatUserDto
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
                ChatUsers = new List<ChatUserDto>();
            }
        }

        private async Task LoadChatHistory(Guid currentUserId, Guid receiverId)
        {
            try
            {
                ChatHistory = await _chatRepository
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
                ChatHistory = new List<Models.Models.Chat>();
            }
        }

        public IActionResult OnGetChatHistory(Guid receiverId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" })
                {
                    StatusCode = 401
                };
            }

            try
            {
                // Get chat history between current user and receiver
                var messages = _chatRepository.GetAllQueryable(
                    c => (c.SenderId == userId.Value && c.ReceiverId == receiverId) ||
                         (c.SenderId == receiverId && c.ReceiverId == userId.Value))
                        .Include(c => c.SenderId)
                        .Include(c => c.ReceiverId)
                        .OrderBy(c => c.Timestamp);

                var chatHistory = messages.Select(m => new
                {
                    senderId = m.SenderId.ToString(),
                    senderName = m.Sender.UserName,
                    message = m.Message,
                    timestamp = m.Timestamp.ToString("o"), // ISO format
                    receiverId = m.ReceiverId.ToString()
                }).ToList();

                return new JsonResult(chatHistory);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading chat history: {ex.Message}");
                return new JsonResult(new { success = false, message = "Failed to load chat history" })
                {
                    StatusCode = 500
                };
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("extension_userId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
        public class ChatUserDto
        {
            public Guid UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string? UserAvatar { get; set; }
            public string LastMessage { get; set; } = string.Empty;
            public DateTime LastMessageTime { get; set; }
            public bool IsArtist { get; set; }
            public int MessageCount { get; set; }
        }
    }
}
