using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

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
        public async Task SendMessage(string receiverId, string messageContent)
        {
            // بنجيب الـ ID بتاع الشخص اللي باعت الرسالة دلوقتي
            var senderId = Context.UserIdentifier; 

            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
                return;

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