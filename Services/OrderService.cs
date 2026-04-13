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

        // ==========================================
        // إنشاء طلب جديد
        // ==========================================
        public async Task<OrderResponseDto?> CreateOrderAsync(CreateOrderDto dto, string userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists) throw new Exception("المستخدم غير موجود");

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) throw new Exception($"المنتج رقم {item.ProductId} غير موجود.");
                if (product.Stock < item.Quantity) throw new Exception($"الكمية المطلوبة من {product.Name} غير متوفرة.");

                totalAmount += (product.Price * item.Quantity);
                product.Stock -= item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Product = product 
                });
            }

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Preparing,
                OrderItems = orderItems,
                ShippingAddress = dto.ShippingAddress, // 👈 أخذ العنوان من المستخدم
                PhoneNumber = dto.PhoneNumber,
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return new OrderResponseDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress, // 👈 إرجاعها في الرد
                PhoneNumber = order.PhoneNumber,
                Status = order.Status.ToString(),
                UserId = order.UserId,
                Items = orderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "N/A",      // إنجليزي
                    ProductNameAR = oi.Product?.NameAR ?? "غير متوفر", // عربي
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }

        // ==========================================
        // جلب طلبات المستخدم الحالي
        // ==========================================
        public async Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                ShippingAddress = o.ShippingAddress, // 👈 جلب من الداتا بيز
                PhoneNumber = o.PhoneNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                UserId = o.UserId,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    
                    ProductNameAR = oi.Product.NameAR, // الاسم بالعربي
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            });
        }

        // ==========================================
        // جلب جميع الطلبات (للسوبر أدمن)
        // ==========================================
        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                ShippingAddress = o.ShippingAddress, // 👈 جلب من الداتا بيز
                PhoneNumber = o.PhoneNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                UserId = o.UserId,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "N/A",
                    ProductNameAR = oi.Product?.NameAR ?? "غير متوفر", // الاسم بالعربي
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
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
    }
}