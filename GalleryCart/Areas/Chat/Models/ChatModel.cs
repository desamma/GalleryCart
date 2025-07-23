using GalleryCart.Models.Models;

namespace GalleryCart.Areas.Chat.Models
{
    public class ChatModel
    {
        public User? CurrentUser { get; set; }
        public List<User> ConnectedUsers { get; set; } = new();
        public User? CurrentReceiver { get; set; }
        public List<ChatUserDto> ChatUsers { get; set; } = new();
        public List<GalleryCart.Models.Models.Chat> ChatHistory { get; set; } = new();

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
