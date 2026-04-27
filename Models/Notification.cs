namespace EcommerceAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // الشخص اللي رايحله الإشعار
        public ApplicationUser? User { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        
        public bool IsRead { get; set; } = false; // هل اتقرت ولا لأ
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // اختياري: عشان لو عايز لما يدوس ع الإشعار يوديه لصفحة الطلب
        public int? RelatedOrderId { get; set; } 
    }
}