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
        // 👇 إضافة خدمة الإشعارات هنا
        private readonly INotificationService _notificationService;

        public OrderService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ==========================================
        // إنشاء طلب جديد
        // ==========================================
        public async Task<OrderResponseDto?> CreateOrderAsync(CreateOrderDto dto, string userId)
        {
            // 👇 تعديل بسيط هنا: جبنا بيانات اليوزر كاملة بدل ما نتأكد إنه موجود بس (عشان نحتاج اسمه في الإشعار)
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("المستخدم غير موجود");

            // ==========================================
            // 1. جلب العنوان المختار والتحقق منه
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
                ShippingAddress = fullShippingAddress, 
                PhoneNumber = dto.PhoneNumber ?? userAddress.PhoneNumber,
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // ==========================================
            // 🔥 4. إرسال الإشعارات (Real-Time)
            // ==========================================
            try
            {
                string orderSummary = string.Join(" و ", orderItems.Select(i => $"{i.Quantity} من {i.Product.Name}"));
                string userName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.UserName;

                // أ. إشعار للسوبر أدمن (بكل تفاصيل الأوردر)
                await _notificationService.SendToSuperAdminsAsync(
                    "طلب جديد 🛒", 
                    $"قام {userName} بطلب جديد يحتوي على: {orderSummary}. العنوان: {fullShippingAddress}، رقم الهاتف: {order.PhoneNumber}", 
                    order.Id);

var distinctAdminIds = orderItems
    .Where(oi => oi.Product != null && !string.IsNullOrEmpty(oi.Product.CreatedByUserId)) 
    .Select(oi => oi.Product.CreatedByUserId)
    .Distinct();

foreach (var adminId in distinctAdminIds)
{
    // هنا كمان غيرناها لـ CreatedByUserId
    var adminProducts = orderItems.Where(oi => oi.Product != null && oi.Product.CreatedByUserId == adminId);
    string adminItemsSummary = string.Join(" و ", adminProducts.Select(i => $"{i.Quantity} من {i.Product.Name}"));

    await _notificationService.SendNotificationAsync(
        adminId,
        "مبيعات جديدة 💰",
        $"تم شراء منتجاتك: {adminItemsSummary} ضمن الطلب رقم #{order.Id}.",
        order.Id);
}
            }
            catch (Exception)
            {
                // الـ Try Catch هنا عشان لو حصل أي عطل في سيرفر الـ SignalR، الطلب ميقعش ويكمل حفظ عادي
            }

            // ==========================================
            // 5. إرجاع النتيجة
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
                    UnitPrice = oi.UnitPrice,
                    VendorId = oi.Product?.CreatedByUserId,
        VendorName = oi.Product?.CreatedByUser?.FullName ?? oi.Product?.CreatedByUser?.UserName ?? "غير معروف",
        VendorPhone = oi.Product?.CreatedByUser?.WhatsAppNumber ?? oi.Product?.CreatedByUser?.PhoneNumber ?? "لا يوجد رقم"
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
                    ProductName = oi.Product?.Name ?? "N/A",
                    ProductNameAR = oi.Product?.NameAR ?? "غير متوفر",
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
                .ThenInclude(p => p.CreatedByUser) // 🔥 أهم سطر: جلب بيانات صاحب المنتج
        .OrderByDescending(o => o.OrderDate)
        .ToListAsync();

    return orders.Select(o => new OrderResponseDto
    {
        OrderId = o.Id,
        // ... (باقي الحقول كما هي) ...
        Items = o.OrderItems.Select(oi => new OrderItemResponseDto
        {
            ProductId = oi.ProductId,
            ProductName = oi.Product?.Name ?? "N/A",
            ProductNameAR = oi.Product?.NameAR ?? "غير متوفر",
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,

            // 👇 ملء بيانات صاحب المنتج هنا
            VendorId = oi.Product?.CreatedByUserId,
            VendorName = oi.Product?.CreatedByUser?.FullName ?? oi.Product?.CreatedByUser?.UserName ?? "غير معروف",
            VendorPhone = oi.Product?.CreatedByUser?.WhatsAppNumber ?? oi.Product?.CreatedByUser?.PhoneNumber ?? "لا يوجد رقم"
        }).ToList()
    });
}
        // ==========================================
        // تحديث حالة الطلب
        // ==========================================
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            // 🔥 إرسال إشعار لليوزر بتحديث حالة الطلب
            try
            {
                await _notificationService.SendNotificationAsync(
                    order.UserId,
                    "تحديث حالة الطلب 📦",
                    $"تم تغيير حالة طلبك رقم #{order.Id} إلى '{newStatus}'.",
                    order.Id);
            }
            catch (Exception)
            {
                // تجاهل أخطاء الإشعارات حتى لا تفشل عملية تحديث الحالة
            }

            return true;
        }
    }
}