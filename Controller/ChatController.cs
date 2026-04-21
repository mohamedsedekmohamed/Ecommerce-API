using EcommerceAPI.Data;
using EcommerceAPI.Models; 
using EcommerceAPI.DTOs.Auth; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // أي مستخدم مسجل يقدر يدخل، بس هنحدد الصلاحيات على كل دالة
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =================================================================================
        // 1. صلاحيات السوبر أدمن (SuperAdmin)
        // =================================================================================

        // جلب كل المحادثات (للسوبر أدمن: بيجيب قايمة المستخدمين اللي كلموه)
        [HttpGet("allchat")]
        [Authorize(Roles = "SuperAdmin")] // للسوبر أدمن فقط
        public async Task<IActionResult> GetAllChats()
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // جلب الـ IDs الخاصة بالمستخدمين الذين تواصلوا مع السوبر أدمن
            var userIds = await _context.ChatMessages
                .Where(m => m.SenderId == myId || m.ReceiverId == myId)
                .Select(m => m.SenderId == myId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            // جلب بيانات المستخدمين مع عدد الرسائل غير المقروءة
            var chats = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new 
                { 
                    UserId = u.Id, 
                    UserName = u.UserName,
                    UnreadCount = _context.ChatMessages.Count(m => m.SenderId == u.Id && m.ReceiverId == myId && !m.IsRead)
                })
                .ToListAsync();

            return Ok(ApiResponse.Success("تم جلب قائمة المحادثات", "All chats fetched", "ar", chats));
        }

        // جلب محادثة مع مستخدم معين بواسطة الـ ID (للسوبر أدمن)
        [HttpGet("chatbyid/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetChatById(string userId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == userId) || 
                            (m.SenderId == userId && m.ReceiverId == myId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new 
                {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Message,
                    m.CreatedAt,
                    m.IsRead,
                    IsMine = m.SenderId == myId
                })
                .ToListAsync();

            // جعل رسائل المستخدم "مقروءة" بمجرد فتح السوبر أدمن للشات
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == userId && m.ReceiverId == myId && !m.IsRead)
                .ToListAsync();
            
            if (unreadMessages.Any())
            {
                unreadMessages.ForEach(m => m.IsRead = true);
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse.Success("تم جلب المحادثة", "Chat fetched", "ar", messages));
        }

        // إرسال رسالة من السوبر أدمن إلى مستخدم معين
        [HttpPost("send-superadmin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SendMessageForSuperAdmin([FromBody] SuperAdminSendDto dto)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(dto.ReceiverId))
                return BadRequest(ApiResponse.Error("يجب تحديد المستلم", "Receiver required", "ar"));

            var newMessage = new ChatMessage
            {
                SenderId = myId,
                ReceiverId = dto.ReceiverId,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _context.ChatMessages.AddAsync(newMessage);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Success("تم إرسال الرسالة للمستخدم", "Sent to User", "ar"));
        }

        // =================================================================================
        // 2. طلبات المستخدم العادي (User)
        // =================================================================================

        // جلب المحادثة الخاصة بي كمستخدم مع السوبر أدمن
        [HttpGet("my-chat")]
        public async Task<IActionResult> GetMyChat()
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var superAdminId = await GetSuperAdminIdAsync();

            if (string.IsNullOrEmpty(superAdminId))
                return NotFound(ApiResponse.Error("الدعم الفني غير متاح حالياً", "SuperAdmin not found", "ar"));

            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == superAdminId) || 
                            (m.SenderId == superAdminId && m.ReceiverId == myId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new 
                {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Message,
                    m.CreatedAt,
                    m.IsRead,
                    IsMine = m.SenderId == myId
                })
                .ToListAsync();

            // جعل رسائل السوبر أدمن "مقروءة" بمجرد فتح المستخدم للشات
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == superAdminId && m.ReceiverId == myId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                unreadMessages.ForEach(m => m.IsRead = true);
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse.Success("تم جلب المحادثة مع الإدارة", "SuperAdmin messages fetched", "ar", messages));
        }

        // إرسال رسالة من المستخدم إلى السوبر أدمن
        [HttpPost("send-user")]
        public async Task<IActionResult> SendMessageForUser([FromBody] UserSendDto dto)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // جلب الـ ID الخاص بالسوبر أدمن تلقائياً لتوجيه الرسالة له
            var superAdminId = await GetSuperAdminIdAsync();

            if (string.IsNullOrEmpty(superAdminId))
                return NotFound(ApiResponse.Error("لا يوجد دعم فني متاح لاستقبال الرسالة", "No SuperAdmin available", "ar"));

            var newMessage = new ChatMessage
            {
                SenderId = myId,
                ReceiverId = superAdminId, // التوجيه الإجباري للسوبر أدمن
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _context.ChatMessages.AddAsync(newMessage);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Success("تم إرسال الرسالة للإدارة", "Sent to SuperAdmin", "ar"));
        }

        // =================================================================================
        // دالة مساعدة (Helper Method) لجلب ID السوبر أدمن تلقائياً
        // =================================================================================
        private async Task<string> GetSuperAdminIdAsync()
        {
            return await (from user in _context.Users
                          join userRole in _context.UserRoles on user.Id equals userRole.UserId
                          join role in _context.Roles on userRole.RoleId equals role.Id
                          where role.Name == "SuperAdmin"
                          select user.Id).FirstOrDefaultAsync();
        }
    }

    // =================================================================================
    // كلاسات الـ DTO
    // =================================================================================

    // DTO للسوبر أدمن (عشان يرد على مستخدم معين، لازم يبعت الـ ID بتاع المستخدم)
    public class SuperAdminSendDto
    {
        public string ReceiverId { get; set; } 
        public string Message { get; set; } 
    }

    // DTO للمستخدم العادي (بيبعت الرسالة بس، والباك إند بيوجهها للسوبر أدمن لوحده)
    public class UserSendDto
    {
        public string Message { get; set; } 
    }
}