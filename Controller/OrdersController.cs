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
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private string GetLang() =>
            Request.Headers["Accept-Language"].ToString();

        // ==========================================
        // Create Order
        // ==========================================
       [HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
{
    var lang = GetLang();

    if (!ModelState.IsValid)
        return BadRequest(ApiResponse.Error("بيانات غير صحيحة", "Invalid data", lang));

    try 
    {
        // سحب الـ UserId من الـ Token عبر الميثود الموجودة في الكنترولر
        var userId = GetUserId(); 

        // إرسال الـ DTO مع الـ UserId للخدمة
        var order = await _orderService.CreateOrderAsync(dto, userId);

        return Ok(ApiResponse.Success(
            "تم إنشاء الطلب بنجاح",
            "Order created successfully",
            lang,
            order));
    }
    catch (Exception ex)
    {
        return BadRequest(ApiResponse.Error(ex.Message, ex.Message, lang));
    }
}

        // ==========================================
        // SuperAdmin - All Orders
        // ==========================================
        [HttpGet("all-orders")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllOrders()
        {
            var lang = GetLang();

            var orders = await _orderService.GetAllOrdersAsync();

            return Ok(ApiResponse.Success(
                "تم جلب جميع الطلبات",
                "All orders fetched successfully",
                lang,
                orders));
        }

        // ==========================================
        // User Orders
        // ==========================================
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var lang = GetLang();

            var orders = await _orderService.GetUserOrdersAsync(GetUserId());

            return Ok(ApiResponse.Success(
                "تم جلب طلباتك",
                "Your orders fetched successfully",
                lang,
                orders));
        }

        // ==========================================
        // Update Status
        // ==========================================
        [HttpPut("{id}/status")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] OrderStatus newStatus)
        {
            var lang = GetLang();

            var isUpdated = await _orderService.UpdateOrderStatusAsync(id, newStatus);

            if (!isUpdated)
                return NotFound(ApiResponse.Error(
                    "الطلب غير موجود",
                    "Order not found",
                    lang));

            return Ok(ApiResponse.Success(
                "تم تحديث حالة الطلب بنجاح",
                "Order status updated successfully",
                lang));
        }
    }
}