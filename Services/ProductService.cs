using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
private readonly UserManager<ApplicationUser> _userManager; // إضافة هذا
      public ProductService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
{
    _context = context;
    _userManager = userManager;
}

        // دالة التحويل (Mapping) مع منطق الخصوصية الصارم
private static ProductDto MapToDto(Product p, bool isSuperAdmin, string? userRole = null) => new ProductDto        {
            Id = p.Id,
            Name = p.Name,
            NameAR = p.NameAR,
            Description = p.Description,
            DescriptionAR = p.DescriptionAR,
            Price = p.Price,
            Currency = p.Currency.ToString(),
            ImageUrl = p.ImageUrl,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? "بدون تصنيف",
            CategoryNameAR = p.Category?.NameAR ?? "بدون تصنيف",

            // هذه البيانات ترجع "Protected" إلا إذا تم تمرير isSuperAdmin = true من الكنترولر
            CreatedByUserId = isSuperAdmin ? p.CreatedByUserId : null,
            CreatedByUserName = isSuperAdmin 
                ? (p.CreatedByUser?.FullName ?? p.CreatedByUser?.UserName ?? "غير معروف") 
                : "Protected",
            
            CreatedByUserPhone = isSuperAdmin 
                ? (p.CreatedByUser?.WhatsAppNumber ?? p.CreatedByUser?.PhoneNumber ?? "لا يوجد رقم") 
                : "Protected",
            
            CreatedByUserEmail = isSuperAdmin 
                ? (p.CreatedByUser?.Email ?? "لا يوجد إيميل") 
                : "Protected",
                CreatedByUserRole = isSuperAdmin ? userRole : "Protected"
        };

     public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(string userId, bool isSuperAdmin)
{
    var query = _context.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .Include(p => p.CreatedByUser)
        .AsQueryable();

    if (!isSuperAdmin && !string.IsNullOrEmpty(userId))
    {
        query = query.Where(p => p.CreatedByUserId == userId);
    }

    var products = await query.ToListAsync();
    
    var dtos = new List<ProductDto>();

    foreach (var p in products)
    {
        string role = "User";
        if (isSuperAdmin && p.CreatedByUser != null)
        {
            // جلب الرتبة فقط لو السائل سوبر أدمن
            var roles = await _userManager.GetRolesAsync(p.CreatedByUser);
            role = roles.FirstOrDefault() ?? "No Role";
        }
        
        dtos.Add(MapToDto(p, isSuperAdmin, role));
    }

    return dtos;
}
             public async Task<ProductDto?> GetProductByIdAsync(int id, string userId, bool isSuperAdmin)
        {
            var p = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (p == null) return null;

            // حماية الملكية للأدمن العادي
            if (!isSuperAdmin && !string.IsNullOrEmpty(userId) && p.CreatedByUserId != userId) return null;

            return MapToDto(p, isSuperAdmin);
        }

        public async Task<ProductDto?> CreateProductAsync(CreateProductDto dto, string userId)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) return null;

            var product = new Product
            {
                Name = dto.Name,
                NameAR = dto.NameAR,
                Description = dto.Description,
                DescriptionAR = dto.DescriptionAR,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                Currency = dto.Currency,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                CreatedByUserId = userId 
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // عند الإنشاء، نرجع البيانات كاملة للمستخدم الذي أنشأها
            return await GetProductByIdAsync(product.Id, userId, true);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, CreateProductDto dto, string userId, bool isSuperAdmin)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            if (!isSuperAdmin && product.CreatedByUserId != userId) return null;

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) return null;

            product.Name = dto.Name;
            product.NameAR = dto.NameAR;
            product.Description = dto.Description;
            product.DescriptionAR = dto.DescriptionAR;
            product.Price = dto.Price;
            product.ImageUrl = dto.ImageUrl;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.Currency = dto.Currency;

            await _context.SaveChangesAsync();
            return await GetProductByIdAsync(product.Id, userId, isSuperAdmin);
        }

        public async Task<bool> DeleteProductAsync(int id, string userId, bool isSuperAdmin)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            if (!isSuperAdmin && product.CreatedByUserId != userId) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string name, string userId, bool isSuperAdmin, bool isActiveOnly)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{name}%") ||
                    EF.Functions.Like(p.NameAR, $"%{name}%"));
            }

            if (!isSuperAdmin && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.CreatedByUserId == userId);
            }

            var result = await query.ToListAsync();
            return result.Select(p => MapToDto(p, isSuperAdmin));
        }
    }
}