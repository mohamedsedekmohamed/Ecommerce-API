using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EcommerceAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // الدالة دي بتشتغل تلقائياً أول ما اليوزر يفتح الموقع ويعمل اتصال بالـ Socket
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // SignalR بيجيب الـ ID من التوكن هنا
            
            _logger.LogInformation($"User {userId} connected to NotificationHub with ConnectionId: {Context.ConnectionId}");

            // ممكن تبعت رسالة ترحيب لليوزر أول ما يدخل (اختياري)
            // await Clients.Caller.SendAsync("ReceiveMessage", "Welcome to real-time notifications!");

            await base.OnConnectedAsync();
        }

        // الدالة دي بتشتغل تلقائياً أول ما اليوزر يقفل الموقع أو النت يفصل عنده
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            
            _logger.LogInformation($"User {userId} disconnected from NotificationHub. ConnectionId: {Context.ConnectionId}");

            await base.OnDisconnectedAsync(exception);
        }
    }
}