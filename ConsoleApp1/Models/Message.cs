namespace AIChat.Models
{
    public partial class Message
    {
        public int Id { get; set; }

        public string SessionId { get; set; } = null!;

        public string Role { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime Created { get; set; }

        public virtual Session Session { get; set; } = null!;
    }
}


