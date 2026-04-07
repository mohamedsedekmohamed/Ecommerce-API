    using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.Enums;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderResponseDto?> CreateOrderAsync(CreateOrderDto dto)
        {
            // 1. التأكد من وجود المستخدم
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists) throw new Exception("المستخدم غير موجود");

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            // 2. المرور على كل المنتجات المطلوبة لحساب السعر وخصم المخزون
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                
                if (product == null) throw new Exception($"المنتج رقم {item.ProductId} غير موجود.");
                if (product.Stock < item.Quantity) throw new Exception($"الكمية المطلوبة من {product.Name} غير متوفرة في المخزون.");

                // حساب الإجمالي
                totalAmount += (product.Price * item.Quantity);

                // خصم المخزون
                product.Stock -= item.Quantity;

                // تجهيز عنصر الطلب للفاتورة
                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price // نأخذ السعر من الداتابيز وليس من المستخدم
                });
            }

            // 3. إنشاء الطلب النهائي
            var order = new Order
            {
                UserId = dto.UserId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Preparing,
                OrderItems = orderItems
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync(); // هنا يتم حفظ الطلب، وعناصر الطلب، وتحديث المخزون معاً!

            return new OrderResponseDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                UserId = order.UserId
            };
        }

    public async Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDto
            {
                OrderId = o.Id, 
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(), 
                UserId = o.UserId
            });
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }
        // 👇 إضافة الدالة الجديدة داخل OrderService
      public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDto
            {
                OrderId = o.Id, // 👈 تم التعديل لتطابق الـ DTO الخاص بك
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(), // 👈 تحويل الـ Enum إلى String
                UserId = o.UserId
            });
        }
    }
}