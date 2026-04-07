using EcommerceAPI.DTOs.Products;
using EcommerceAPI.DTOs.Categories; // 👈 أضفنا هذا الـ Namespace
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
        private readonly ICategoryService _categoryService; // 👈 1. حقن خدمة التصنيفات هنا

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService; // 👈 2. تعيين الخدمة
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        private bool IsSuperAdmin() => User.IsInRole(AppRoles.SuperAdmin);

       [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> GetAll()
        {
            // 1. جلب المنتجات (بالصلاحيات والفلترة)
            var products = await _productService.GetAllProductsAsync(GetUserId(), IsSuperAdmin());
            
            // 2. جلب التصنيفات الخفيفة للقائمة المنسدلة
            var categories = await _categoryService.GetCategoriesForSelectAsync();

            // 3. دمجهم معاً في الـ DTO الجديد
            var dashboardData = new ProductsDashboardDto
            {
                Products = products,
                Categories = categories
            };

            return Ok(dashboardData);
        }

       

        [HttpGet("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id, GetUserId(), IsSuperAdmin());
            if (product == null) return NotFound($"المنتج غير موجود أو ليس لديك صلاحية لرؤيته.");
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var product = await _productService.CreateProductAsync(dto, GetUserId());
            if (product == null) return BadRequest("التصنيف المحدد غير موجود.");
            return Ok(product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var product = await _productService.UpdateProductAsync(id, dto, GetUserId(), IsSuperAdmin());
            if (product == null) return NotFound($"المنتج غير موجود، التصنيف خاطئ، أو ليس لديك صلاحية لتعديله.");
            return Ok(product);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _productService.DeleteProductAsync(id, GetUserId(), IsSuperAdmin());
            if (!isDeleted) return NotFound($"المنتج رقم {id} غير موجود أو ليس لديك صلاحية لحذفه.");
            return Ok("تم حذف المنتج بنجاح.");
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProducts()
        {
            var products = await _productService.GetAllProductsAsync(string.Empty, true);
            return Ok(products);
        }
    }
}