using EcommerceAPI.DTOs.Products;

namespace EcommerceAPI.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(string userId, bool isSuperAdmin);
        Task<ProductDto?> GetProductByIdAsync(int id, string userId, bool isSuperAdmin);
        Task<ProductDto?> CreateProductAsync(CreateProductDto dto, string userId);
        Task<ProductDto?> UpdateProductAsync(int id, CreateProductDto dto, string userId, bool isSuperAdmin);
        Task<bool> DeleteProductAsync(int id, string userId, bool isSuperAdmin);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string name, string userId, bool isSuperAdmin, bool isActiveOnly);
    }
}