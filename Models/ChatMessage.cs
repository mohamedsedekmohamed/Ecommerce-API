namespace EcommerceAPI.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        
        // من أرسل الرسالة؟
        public string SenderId { get; set; } 
        
        // لمين مبعوته؟ (للأدمن أو للمستخدم)
        public string ReceiverId { get; set; } 
        
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }

        // Navigation Properties (اختياري بس بيساعد لو عايز تجيب اسم المرسل)
        public ApplicationUser Sender { get; set; }
        public ApplicationUser Receiver { get; set; }
    }
}