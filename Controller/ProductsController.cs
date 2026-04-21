using EcommerceAPI.DTOs.Products;
using EcommerceAPI.DTOs.Categories;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Constants;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private bool IsSuperAdmin() =>
            User.IsInRole(AppRoles.SuperAdmin);

        private string GetLang() =>
            Request.Headers["Accept-Language"].ToString();

        [HttpGet("user/all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsForUser()
        {
            var lang = GetLang();

            var products = await _productService.GetAllProductsAsync(string.Empty, true);

            return Ok(ApiResponse.Success(
                "تم جلب المنتجات بنجاح",
                "Products fetched successfully",
                lang,
                products));
        }

        // ==========================================
        // Admin
        // ==========================================
        [HttpGet("admin/all")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> GetProductsForAdmin()
        {
            var lang = GetLang();

            var products = await _productService.GetAllProductsAsync(GetUserId(), false);
            var categories = await _categoryService.GetCategoriesForSelectAsync();

            var data = new ProductsDashboardDto
            {
                Products = products,
                Categories = categories
            };

            return Ok(ApiResponse.Success(
                "تم جلب بيانات المنتجات",
                "Products dashboard fetched",
                lang,
                data));
        }

        // ==========================================
        // SuperAdmin
        // ==========================================
        [HttpGet("superadmin/all")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetProductsForSuperAdmin()
        {
            var lang = GetLang();

            var products = await _productService.GetAllProductsAsync(GetUserId(), true);
            var categories = await _categoryService.GetCategoriesForSelectAsync();

            var data = new ProductsDashboardDto
            {
                Products = products,
                Categories = categories
            };

            return Ok(ApiResponse.Success(
                "تم جلب كل المنتجات",
                "All products fetched",
                lang,
                data));
        }

        // ==========================================
        // Get By Id
        // ==========================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var lang = GetLang();

            var product = await _productService.GetProductByIdAsync(id, GetUserId(), IsSuperAdmin());

            if (product == null)
                return NotFound(ApiResponse.Error(
                    "المنتج غير موجود أو ليس لديك صلاحية",
                    "Product not found or access denied",
                    lang));

            return Ok(ApiResponse.Success(
                "تم جلب المنتج بنجاح",
                "Product fetched successfully",
                lang,
                product));
        }

        // ==========================================
        // Create
        // ==========================================
        [HttpPost]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var lang = GetLang();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error(
                    "بيانات غير صحيحة",
                    "Invalid data",
                    lang));

            var product = await _productService.CreateProductAsync(dto, GetUserId());

            if (product == null)
                return BadRequest(ApiResponse.Error(
                    "التصنيف غير موجود",
                    "Category not found",
                    lang));

            return Ok(ApiResponse.Success(
                "تم إنشاء المنتج بنجاح",
                "Product created successfully",
                lang,
                product));
        }

        // ==========================================
        // Update
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
        {
            var lang = GetLang();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error(
                    "بيانات غير صحيحة",
                    "Invalid data",
                    lang));

            var product = await _productService.UpdateProductAsync(id, dto, GetUserId(), IsSuperAdmin());

            if (product == null)
                return NotFound(ApiResponse.Error(
                    "المنتج غير موجود أو ليس لديك صلاحية أو التصنيف خطأ",
                    "Product not found, invalid category, or access denied",
                    lang));

            return Ok(ApiResponse.Success(
                "تم تحديث المنتج بنجاح",
                "Product updated successfully",
                lang,
                product));
        }

        // ==========================================
        // Delete
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var lang = GetLang();

            var isDeleted = await _productService.DeleteProductAsync(id, GetUserId(), IsSuperAdmin());

            if (!isDeleted)
                return NotFound(ApiResponse.Error(
                    $"المنتج رقم {id} غير موجود أو ليس لديك صلاحية",
                    $"Product with id {id} not found or access denied",
                    lang));

            return Ok(ApiResponse.Success(
                "تم حذف المنتج بنجاح",
                "Product deleted successfully",
                lang));
        }


        [HttpGet("user/search")]
[AllowAnonymous]
public async Task<IActionResult> SearchForUser([FromQuery] string name)
{
    var lang = GetLang();

    var products = await _productService.SearchProductsAsync(
        name,
        string.Empty,
        false,
        true // 👈 active فقط
    );

    return Ok(ApiResponse.Success(
        "تم البحث بنجاح",
        "Search completed",
        lang,
        products));
}
[HttpGet("admin/search")]
[Authorize(Roles = AppRoles.Admin)]
public async Task<IActionResult> SearchForAdmin([FromQuery] string name)
{
    var lang = GetLang();

    var products = await _productService.SearchProductsAsync(
        name,
        GetUserId(),
        false,
        false
    );

    return Ok(ApiResponse.Success(
        "تم البحث بنجاح",
        "Search completed",
        lang,
        products));
}
[HttpGet("superadmin/search")]
[Authorize(Roles = AppRoles.SuperAdmin)]
public async Task<IActionResult> SearchForSuperAdmin([FromQuery] string name)
{
    var lang = GetLang();

    var products = await _productService.SearchProductsAsync(
        name,
        GetUserId(),
        true,
        false
    );

    return Ok(ApiResponse.Success(
        "تم البحث بنجاح",
        "Search completed",
        lang,
        products));
}
    }
}