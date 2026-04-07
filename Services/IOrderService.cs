using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.Enums;

namespace EcommerceAPI.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto?> CreateOrderAsync(CreateOrderDto dto);
        Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(string userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        
        // 👇 السطر الجديد الخاص بالسوبر أدمن لجلب كل الأوردرات
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
    }
}