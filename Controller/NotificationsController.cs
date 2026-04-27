using EcommerceAPI.Constants;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // 1. جلب كل الإشعارات
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(GetUserId());
            return Ok(ApiResponse.Success("تم الجلب", "Fetched", "", notifications));
        }

        // 2. عدد الإشعارات الغير مقروءة (للنقطة الحمرا)
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(GetUserId());
            return Ok(ApiResponse.Success("تم الجلب", "Fetched", "", new 
            { 
                count = count,
                hasUnread = count > 0 // bool عشان لو الفرونت عايز يتعامل بيها
            }));
        }

        // 3. قراءة الإشعار
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var success = await _notificationService.MarkAsReadAsync(id, GetUserId());
            if (!success) return NotFound(ApiResponse.Error("الإشعار غير موجود", "Not found", ""));
            
            return Ok(ApiResponse.Success("تمت القراءة", "Marked as read", ""));
        }
    }
}