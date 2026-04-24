using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.Hubs
{
    [Authorize] // لازم يكون مسجل دخول عشان يبعت رسايل
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // دالة إرسال الرسالة (الواجهة الأمامية هتنادي عليها)
        public async Task SendMessage(string? receiverId, string messageContent)
        {
            // بنجيب الـ ID بتاع الشخص اللي باعت الرسالة دلوقتي
            var senderId = Context.UserIdentifier; 

            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(messageContent))
                return;

            // إذا كان المستخدم عادياً (لم يمرر receiverId)، نرسل الرسالة للسوبر أدمن تلقائياً
            if (string.IsNullOrEmpty(receiverId))
            {
                receiverId = await (from user in _context.Users
                                    join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                    join role in _context.Roles on userRole.RoleId equals role.Id
                                    where role.Name == "SuperAdmin"
                                    select user.Id).FirstOrDefaultAsync();

                // لو مفيش سوبر أدمن في الداتابيز، نوقف العملية
                if (string.IsNullOrEmpty(receiverId)) 
                    return; 
            }

            // 1. حفظ الرسالة في قاعدة البيانات
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = messageContent,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // 2. إرسال الرسالة لحظياً للطرف التاني (لو كان فاتح التطبيق)
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, messageContent, chatMessage.CreatedAt);
        }
    }
}