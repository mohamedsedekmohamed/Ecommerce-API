using EcommerceAPI.DTOs.Categories; // لا تنسَ استدعاء مسار التصنيفات

namespace EcommerceAPI.DTOs.Products
{
    public class ProductsDashboardDto
    {
        public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
        public IEnumerable<CategorySelectDto> Categories { get; set; } = new List<CategorySelectDto>();
    }
}