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

    // ==========================================
    // 👈 1. جلب العنوان المختار والتحقق منه
    // ==========================================
    var userAddress = await _context.Addresses
        .FirstOrDefaultAsync(a => a.Id == dto.AddressId && a.UserId == userId);

    if (userAddress == null) 
        throw new Exception("العنوان المختار غير موجود أو لا يخص هذا المستخدم.");

    // تجميع العنوان في نص واحد لحفظه في الطلب
    string fullShippingAddress = $"{userAddress.Street}, {userAddress.City}, {userAddress.State}";
    string Latitude = userAddress.Latitude;
    string Longitude = userAddress.Longitude;
    decimal totalAmount = 0;
    var orderItems = new List<OrderItem>();

    // ==========================================
    // 2. التحقق من المنتجات
    // ==========================================
    foreach (var item in dto.Items)
    {
        var product = await _context.Products.FindAsync(item.ProductId);
        if (product == null) throw new Exception($"المنتج رقم {item.ProductId} غير موجود.");
        if (product.Stock < item.Quantity) throw new Exception($"الكمية المطلوبة من {product.Name} غير متوفرة.");

        totalAmount += (product.Price * item.Quantity);
        product.Stock -= item.Quantity; // تقليل المخزون

        orderItems.Add(new OrderItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = product.Price,
            Product = product 
        });
    }

    // ==========================================
    // 3. إنشاء الطلب
    // ==========================================
    var order = new Order
    {
        UserId = userId,
        TotalAmount = totalAmount,
        Status = OrderStatus.Preparing,
        OrderItems = orderItems,
        Latitude = Latitude,
        Longitude = Longitude,
        // 👈 نضع العنوان المجمع هنا كـ Text
        ShippingAddress = fullShippingAddress, 
        
        // يمكنك إما أخذ رقم الهاتف من الـ DTO أو من العنوان المختار
        PhoneNumber = dto.PhoneNumber ?? userAddress.PhoneNumber,
    };

    await _context.Orders.AddAsync(order);
    await _context.SaveChangesAsync();

    // ==========================================
    // 4. إرجاع النتيجة
    // ==========================================
    return new OrderResponseDto
    {
        OrderId = order.Id,
        OrderDate = order.OrderDate,
        TotalAmount = order.TotalAmount,
        ShippingAddress = order.ShippingAddress, 
        PhoneNumber = order.PhoneNumber,
        Status = order.Status.ToString(),
        UserId = order.UserId,
        Latitude = order.Latitude,
        Longitude = order.Longitude,
        Items = orderItems.Select(oi => new OrderItemResponseDto
        {
            ProductId = oi.ProductId,
            ProductName = oi.Product?.Name ?? "N/A",      
            ProductNameAR = oi.Product?.NameAR ?? "غير متوفر", 
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice
        }).ToList()
    };
}
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
                ShippingAddress = o.ShippingAddress,
Latitude = o.Latitude,
Longitude = o.Longitude,
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
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                ShippingAddress = o.ShippingAddress,
                Latitude = o.Latitude,
                Longitude = o.Longitude,
                PhoneNumber = o.PhoneNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                UserId = o.UserId,
                User = o.User != null ? new UserInfoDto 
                    {
            Id = o.User.Id,
            FullName = o.User.UserName, 
            Email = o.User.Email
               } : null,
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