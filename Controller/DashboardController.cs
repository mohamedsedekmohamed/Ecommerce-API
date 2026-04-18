using EcommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SummaryResponseDto>> GetSummary()
    {
        // 1. الإحصائيات الأساسية
        var categoriesCount = await _context.Categories.CountAsync();
        var productsCount = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        // 2. إجمالي الأرباح
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);

        // 3. الطلبات قيد الانتظار
        var pendingOrders = await _context.Orders
            .CountAsync(o => o.Status == EcommerceAPI.Enums.OrderStatus.Preparing);        

        // 4. المنتجات التي نفذت من المخزون
        var outOfStockProducts = await _context.Products.CountAsync(p => p.Stock <= 0);

        // 5. أكثر مستخدم طلبات (إرجاع الاسم)
        // نستخدم GroupBy مع الـ Key واسم المستخدم لضمان التجميع الصحيح
        var topUserName = await _context.Orders
            .Where(o => o.User != null) // تأمين في حال وجود طلب بدون مستخدم
            .GroupBy(o => new { o.UserId, o.User.UserName }) // استبدل UserName بـ FullName إذا كان هذا اسم الحقل عندك
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key.UserName)
            .FirstOrDefaultAsync();

        // 6. أكثر عنصر (منتج) يُطلب (إرجاع الاسم)
        var topProductName = await _context.OrderItems
            .Where(oi => oi.Product != null)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name }) // استبدل Name بـ Title أو الحقل المناسب عندك
            .OrderByDescending(g => g.Sum(oi => oi.Quantity))
            .Select(g => g.Key.Name)
            .FirstOrDefaultAsync();

        // 7. تجميع البيانات في الكائن النهائي
        var summary = new SummaryResponseDto
        {
            TotalCategories = categoriesCount,
            TotalProducts = productsCount,
            TotalOrders = totalOrders,
            TotalUsers = totalUsers,
            TotalRevenue = totalRevenue,
            PendingOrders = pendingOrders,
            OutOfStockProducts = outOfStockProducts,
            TopUserName = topUserName, // تمرير الاسم
            TopProductName = topProductName // تمرير الاسم
        };

        return Ok(summary);
    }
}