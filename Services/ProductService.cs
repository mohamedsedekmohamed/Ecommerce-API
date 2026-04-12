using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(string userId, bool isSuperAdmin)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

       
            if (!isSuperAdmin && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.CreatedByUserId == userId);
            }

            var products = await query.ToListAsync();
            
        return products.Select(p => new ProductDto
{
    Id = p.Id,

    Name = p.Name,
    NameAR = p.NameAR,

    Description = p.Description,
    DescriptionAR = p.DescriptionAR,

    Price = p.Price,
    ImageUrl = p.ImageUrl,
    Stock = p.Stock,
    CategoryId = p.CategoryId,

    CategoryName = p.Category?.Name ?? "بدون تصنيف",
    CategoryNameAR = p.Category?.NameAR ?? "بدون تصنيف"
});
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, string userId, bool isSuperAdmin)
        {
            var p = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            
            if (p == null) return null;

            // 👈 حماية: لو مش سوبر أدمن، والمنتج ده مش بتاعه، نرجع null كأنه مش موجود
            if (!isSuperAdmin && p.CreatedByUserId != userId) return null;

            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "بدون تصنيف"
            };
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
    Stock = dto.Stock,
    CategoryId = dto.CategoryId,
    CreatedByUserId = userId
};

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // لجلب بيانات الـ DTO نمرر isSuperAdmin = true مؤقتاً لضمان جلبه بعد الإضافة
            return await GetProductByIdAsync(product.Id, userId, true); 
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, CreateProductDto dto, string userId, bool isSuperAdmin)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            // 👈 حماية التعديل
            if (!isSuperAdmin && product.CreatedByUserId != userId) return null;

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) return null;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.ImageUrl = dto.ImageUrl;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.NameAR = dto.NameAR;
            product.DescriptionAR = dto.DescriptionAR;


            await _context.SaveChangesAsync();
            return await GetProductByIdAsync(product.Id, userId, true);
        }

        public async Task<bool> DeleteProductAsync(int id, string userId, bool isSuperAdmin)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            // 👈 حماية الحذف
            if (!isSuperAdmin && product.CreatedByUserId != userId) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}