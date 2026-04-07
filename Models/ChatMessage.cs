namespace EcommerceAPI.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsAdminMessage { get; set; } // لمعرفة هل الرسالة من الإدارة أم من العميل

        // علاقة: الرسالة تنتمي لمستخدم معين (صاحب المشكلة أو الاستفسار)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}