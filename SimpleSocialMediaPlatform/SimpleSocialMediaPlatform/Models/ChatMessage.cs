namespace SimpleSocialMediaPlatform.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
