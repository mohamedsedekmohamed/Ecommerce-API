using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Auth; // لمكان الـ ApiResponse
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // جلب المحادثة بيني وبين مستخدم آخر (الأدمن يبعت ID المستخدم، والمستخدم يبعت ID الأدمن)
        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(string otherUserId)
        {
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == otherUserId) || 
                            (m.SenderId == otherUserId && m.ReceiverId == myId))
                .OrderBy(m => m.CreatedAt) // ترتيب زمني
                .Select(m => new 
                {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Message,
                    m.CreatedAt,
                    IsMine = m.SenderId == myId // علشان الواجهة تعرف تحط الرسالة يمين ولا شمال
                })
                .ToListAsync();

            return Ok(ApiResponse.Success("تم جلب الرسايل", "Messages fetched", "ar", messages));
        }
    }
}