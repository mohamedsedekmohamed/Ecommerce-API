using EcommerceAPI.Constants;
using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.Enums;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يجب أن يكون المستخدم مسجلاً للدخول للوصول للأوردرات
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // مساعدة: استخراج الـ ID الخاص بالمستخدم الحالي من التوكن
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // 1. إنشاء طلب جديد (متاح لجميع المستخدمين المسجلين)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                // نمرر الـ UserId المستخرج من التوكن لضمان أن الأوردر يتسجل باسم صاحبه
                var order = await _orderService.CreateOrderAsync(dto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 2. السوبر أدمن يشوف كل الأوردرات في النظام
        [HttpGet("all-orders")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllOrders()
        {
            // نفترض وجود دالة GetAllOrdersAsync في الـ Service
            var orders = await _orderService.GetAllOrdersAsync(); 
            return Ok(orders);
        }

        // 3. المستخدم يشوف الأوردرات الخاصة به فقط
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            // نستخدم الـ ID المستخرج من التوكن لضمان الخصوصية
            var orders = await _orderService.GetUserOrdersAsync(GetUserId());
            return Ok(orders);
        }

        // 4. السوبر أدمن فقط هو من يعدل حالة الأوردر (قيد التنفيذ، تم الشحن، إلخ)
        [HttpPut("{id}/status")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] OrderStatus newStatus)
        {
            var isUpdated = await _orderService.UpdateOrderStatusAsync(id, newStatus);
            if (!isUpdated) return NotFound("الطلب غير موجود.");

            return Ok(new { Message = "تم تحديث حالة الطلب بنجاح." });
        }
    }
}