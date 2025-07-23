using GalleryCart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace GalleryCart.Chat
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;

        //all connected users
        private static readonly ConcurrentDictionary<string, ConnectedUser> ConnectedUsers = new();

        public ChatHub(IUserRepository userRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var identity = Context.User?.Identity as ClaimsIdentity;
                var userIdClaim = identity?.Claims.FirstOrDefault(c => c.Type == "extension_userId")?.Value
                    ?? identity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid user authentication");
                    return;
                }

                var userInfo = await _userRepository.GetAsync(u => u.Id == userId);
                if (userInfo == null)
                {
                    await Clients.Caller.SendAsync("Error", "User not found");
                    return;
                }

                // Remove any existing connections for this user
                var existingConnections = ConnectedUsers.Where(kvp => kvp.Value.UserId == userId).ToList();
                foreach (var existing in existingConnections)
                {
                    ConnectedUsers.TryRemove(existing.Key, out _);
                }

                // Store the user info in memory, including ConnectionId
                var connectedUser = new ConnectedUser
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userInfo.Id,
                    UserName = userInfo.UserName ?? "Unknown",
                    PhoneNumber = userInfo.PhoneNumber ?? string.Empty,
                    UserAvatar = userInfo.UserAvatar ?? string.Empty,
                    IsArtist = userInfo.IsArtits
                };

                ConnectedUsers.TryAdd(Context.ConnectionId, connectedUser);

                // Notify all clients about updated user list
                await Clients.All.SendAsync("UpdatedConnectedUsers", ConnectedUsers.Values.ToList());

                // Welcome the new user
                await Clients.Caller.SendAsync("Connected", "Successfully connected to chat");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Connection error: {ex.Message}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                if (ConnectedUsers.TryRemove(Context.ConnectionId, out var user))
                {
                    await Clients.All.SendAsync("UpdatedConnectedUsers", ConnectedUsers.Values.ToList());
                    await Clients.All.SendAsync("UserDisconnected", user.UserId, user.UserName);
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw
                Console.WriteLine($"Disconnection error: {ex.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverId, string message)
        {
            try
            {
                if (!ConnectedUsers.TryGetValue(Context.ConnectionId, out var sender))
                {
                    await Clients.Caller.SendAsync("Error", "Sender not found");
                    return;
                }

                if (string.IsNullOrWhiteSpace(message) || message.Length > 500)
                {
                    await Clients.Caller.SendAsync("Error", "Invalid message");
                    return;
                }

                if (!Guid.TryParse(receiverId, out var receiverGuid))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid receiver ID");
                    return;
                }

                // Save message to database
                var chatMessage = new Models.Models.Chat
                {
                    ChatId = Guid.NewGuid(),
                    SenderId = sender.UserId,
                    ReceiverId = receiverGuid,
                    Message = message.Trim(),
                    Timestamp = DateTime.Now
                };

                await _chatRepository.AddAsync(chatMessage);

                Console.WriteLine($"Message saved to DB: {sender.UserName} -> {receiverId}: {message}");

                // Send to receiver if online
                var receiver = ConnectedUsers.Values.FirstOrDefault(u => u.UserId == receiverGuid);
                if (receiver != null)
                {
                    Console.WriteLine($"Sending message to receiver: {receiver.UserName}");
                    await Clients.Client(receiver.ConnectionId).SendAsync("ReceiveMessage",
                        sender.UserId.ToString(), sender.UserName, message, chatMessage.Timestamp.ToString("o"));
                }
                else
                {
                    Console.WriteLine($"Receiver not online: {receiverId}");
                }

                // Send confirmation to sender (this tells the sender the message was processed)
                await Clients.Caller.SendAsync("MessageSent", receiverId, message, chatMessage.Timestamp.ToString("o"));

                Console.WriteLine($"Message sent successfully from {sender.UserName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage error: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        public async Task GetChatHistory(string receiverId)
        {
            try
            {
                if (!ConnectedUsers.TryGetValue(Context.ConnectionId, out var sender))
                {
                    await Clients.Caller.SendAsync("Error", "Unauthorized");
                    return;
                }

                if (!Guid.TryParse(receiverId, out var receiverGuid))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid receiver ID");
                    return;
                }

                // Get chat history between two users
                var messages = await _chatRepository
                    .GetAllQueryable(c => (c.SenderId == sender.UserId && c.ReceiverId == receiverGuid) ||
                                         (c.SenderId == receiverGuid && c.ReceiverId == sender.UserId))
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

                await Clients.Caller.SendAsync("ChatHistory", chatHistory);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to get chat history: {ex.Message}");
            }
        }

        public async Task GetConversations()
        {
            try
            {
                if (!ConnectedUsers.TryGetValue(Context.ConnectionId, out var user))
                {
                    await Clients.Caller.SendAsync("Error", "Unauthorized");
                    return;
                }

                // Get all messages where the user is either sender or receiver
                var userMessages = await _chatRepository
                    .GetAllQueryable(c => c.SenderId == user.UserId || c.ReceiverId == user.UserId)
                    .Include(c => c.Sender)
                    .Include(c => c.Receiver)
                    .ToListAsync();

                // Group by conversation partner and get the latest message for each
                var conversations = userMessages
                    .GroupBy(c => c.SenderId == user.UserId ? c.ReceiverId : c.SenderId)
                    .Select(g => g.OrderByDescending(c => c.Timestamp).First())
                    .OrderByDescending(c => c.Timestamp)
                    .Select(c => new
                    {
                        userId = c.SenderId == user.UserId ? c.ReceiverId : c.SenderId,
                        userName = c.SenderId == user.UserId ? c.Receiver?.UserName : c.Sender?.UserName,
                        userAvatar = c.SenderId == user.UserId ? c.Receiver?.UserAvatar : c.Sender?.UserAvatar,
                        lastMessage = c.Message,
                        lastMessageTime = c.Timestamp
                    })
                    .ToList();

                await Clients.Caller.SendAsync("ConversationsList", conversations);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to get conversations: {ex.Message}");
            }
        }

        public async Task UserTyping(string receiverId, bool isTyping)
        {
            try
            {
                if (!ConnectedUsers.TryGetValue(Context.ConnectionId, out var sender))
                {
                    return;
                }

                if (!Guid.TryParse(receiverId, out var receiverGuid))
                {
                    return;
                }

                var receiver = ConnectedUsers.Values.FirstOrDefault(u => u.UserId == receiverGuid);
                if (receiver != null)
                {
                    await Clients.Client(receiver.ConnectionId).SendAsync("UserTyping",
                        sender.UserId.ToString(), sender.UserName, isTyping);
                }
            }
            catch (Exception ex)
            {
                // Log but don't send error for typing indicators
                Console.WriteLine($"Typing indicator error: {ex.Message}");
            }
        }

        public async Task JoinRoom(string roomName)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

                if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var user))
                {
                    await Clients.Group(roomName).SendAsync("UserJoinedRoom", user.UserId, user.UserName, roomName);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to join room: {ex.Message}");
            }
        }

        public async Task LeaveRoom(string roomName)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

                if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var user))
                {
                    await Clients.Group(roomName).SendAsync("UserLeftRoom", user.UserId, user.UserName, roomName);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to leave room: {ex.Message}");
            }
        }

        public async Task GetOnlineUsers()
        {
            try
            {
                await Clients.Caller.SendAsync("UpdatedConnectedUsers", ConnectedUsers.Values.ToList());
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to get online users: {ex.Message}");
            }
        }

        private class ConnectedUser
        {
            public string ConnectionId { get; set; } = string.Empty;
            public Guid UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string UserAvatar { get; set; } = string.Empty;
            public bool IsArtist { get; set; } = false;
            public DateTime ConnectedAt { get; set; } = DateTime.Now;
        }
    }
}