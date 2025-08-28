using AIChat.Service;

namespace AIChat.Models
{

    public class SessionInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ChatService ChatService { get; set; } = null!;
    }
}
