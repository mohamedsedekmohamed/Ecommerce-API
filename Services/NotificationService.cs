using EcommerceAPI.Constants;
using EcommerceAPI.Data;
using EcommerceAPI.Hubs;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, int? orderId = null)
        {
            // 1. حفظ في الداتابيز
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                RelatedOrderId = orderId
            };
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            // 2. إرسال Real-time
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new 
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                createdAt = notification.CreatedAt,
                isRead = notification.IsRead,
                relatedOrderId = notification.RelatedOrderId
            });
        }

        public async Task SendToSuperAdminsAsync(string title, string message, int? orderId = null)
        {
            var superAdmins = await _userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);
            foreach (var admin in superAdmins)
            {
                await SendNotificationAsync(admin.Id, title, message, orderId);
            }
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}