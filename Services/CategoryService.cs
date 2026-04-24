using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Categories;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            // تحويل الـ Model إلى DTO
         return categories.Select(c => new CategoryDto
{
    Id = c.Id,
    Name = c.Name,
    NameAR = c.NameAR,
    Description = c.Description,
    DescriptionAR = c.DescriptionAR,
    Icon = c.Icon

});
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

         return new CategoryDto
{
    Id = category.Id,
    Name = category.Name,
    NameAR = category.NameAR,
    Description = category.Description,
    DescriptionAR = category.DescriptionAR,
    Icon = category.Icon
};
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
  Name = dto.Name,
        NameAR = dto.NameAR,
        Description = dto.Description,
        DescriptionAR = dto.DescriptionAR,
        Icon = dto.Icon
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync(); // حفظ التغييرات في قاعدة البيانات

            return new CategoryDto
            {
                 Name = dto.Name,
    NameAR = dto.NameAR,
    Description = dto.Description,
    DescriptionAR = dto.DescriptionAR,
                 Icon = dto.Icon,
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.NameAR = dto.NameAR;
            category.DescriptionAR = dto.DescriptionAR;

            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                NameAR = category.NameAR,
                Description = category.Description,
                DescriptionAR = category.DescriptionAR,
                Icon = category.Icon
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
      public async Task<IEnumerable<CategorySelectDto>> GetCategoriesForSelectAsync()
{
    return await _context.Categories
        .Select(c => new CategorySelectDto
        {
            Id = c.Id,
            Name = c.Name,
            NameAR = c.NameAR,
            Icon = c.Icon,
            ProductsCount = _context.Products.Count(p => p.CategoryId == c.Id) // 👈 هنا
        })
        .ToListAsync();
}
    }
}