namespace AIChat.Models
{

    public partial class Session
    {
        public string SessionId { get; set; } = null!;

        public string? Title { get; set; }

        public DateTime Created { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}