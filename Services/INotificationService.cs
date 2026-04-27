using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface INotificationService
    {
        // إرسال إشعار لمستخدم معين
        Task SendNotificationAsync(string userId, string title, string message, int? orderId = null);
        
        // إرسال إشعار لكل السوبر أدمنز
        Task SendToSuperAdminsAsync(string title, string message, int? orderId = null);
        
        // جلب إشعارات المستخدم
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        
        // عدد الإشعارات غير المقروءة (عشان النقطة الحمرا)
        Task<int> GetUnreadCountAsync(string userId);
        
        // قراءة إشعار
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
    }
}