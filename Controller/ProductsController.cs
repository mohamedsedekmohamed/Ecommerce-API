using EcommerceAPI.DTOs.Products;
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

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        private bool IsSuperAdmin() => User.IsInRole(AppRoles.SuperAdmin);
        private string GetLang() => Request.Headers["Accept-Language"].ToString();

        // ---------------------------------------------------------
        // 1. الواجهة العامة (Public API) - بيانات محمية للجميع
        // ---------------------------------------------------------
        [HttpGet("user/all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsForUser()
        {
            var lang = GetLang();
            
            // تمرير false دائماً لضمان عدم ظهور أرقام الهواتف في الواجهة العامة
            // حتى لو كان السوبر أدمن هو من يتصفح كزائر
            var products = await _productService.GetAllProductsAsync(string.Empty, false);

            return Ok(ApiResponse.Success("تم جلب المنتجات بنجاح", "Products fetched successfully", lang, products));
        }

        // ---------------------------------------------------------
        // 2. لوحة تحكم الأدمن (منتجاته فقط - بيانات محمية)
        // ---------------------------------------------------------
        [HttpGet("admin/all")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> GetProductsForAdmin()
        {
            var lang = GetLang();
            var products = await _productService.GetAllProductsAsync(GetUserId(), false);
            var categories = await _categoryService.GetCategoriesForSelectAsync();

            var data = new ProductsDashboardDto { Products = products, Categories = categories };
            return Ok(ApiResponse.Success("تم جلب بيانات المنتجات", "Products dashboard fetched", lang, data));
        }

        // ---------------------------------------------------------
        // 3. لوحة تحكم السوبر أدمن (كل المنتجات - داتا كاملة)
        // ---------------------------------------------------------
        [HttpGet("superadmin/all")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetProductsForSuperAdmin()
        {
            var lang = GetLang();
            // نمرر true هنا فقط لعرض الأرقام والإيميلات للسوبر أدمن
            var products = await _productService.GetAllProductsAsync(GetUserId(), true);
            var categories = await _categoryService.GetCategoriesForSelectAsync();

            var data = new ProductsDashboardDto { Products = products, Categories = categories };
            return Ok(ApiResponse.Success("تم جلب كل المنتجات", "All products fetched", lang, data));
        }

        // بقية الدوال (Create, Update, Delete) تظل كما هي مع تمرير IsSuperAdmin() في الـ Update/Delete
        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
        {
            var lang = GetLang();
            var product = await _productService.UpdateProductAsync(id, dto, GetUserId(), IsSuperAdmin());
            if (product == null) return NotFound(ApiResponse.Error("المنتج غير موجود", "Product not found", lang));
            return Ok(ApiResponse.Success("تم التحديث بنجاح", "Updated successfully", lang, product));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var lang = GetLang();
            var isDeleted = await _productService.DeleteProductAsync(id, GetUserId(), IsSuperAdmin());
            if (!isDeleted) return NotFound(ApiResponse.Error("لم يتم الحذف", "Delete failed", lang));
            return Ok(ApiResponse.Success("تم الحذف بنجاح", "Deleted successfully", lang));
        }
        [HttpPost]
[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    var lang = GetLang();

    // 1. التحقق من صحة البيانات المرسلة (Validation)
    if (!ModelState.IsValid) 
        return BadRequest(ApiResponse.Error("بيانات المنتج غير مكتملة", "Invalid product data", lang));

    // 2. استدعاء السيرفس لإنشاء المنتج
    // نمرر GetUserId() لربط المنتج بالمستخدم الذي قام بإنشائه
    var product = await _productService.CreateProductAsync(dto, GetUserId());

    if (product == null) 
        return BadRequest(ApiResponse.Error("التصنيف غير موجود", "Category not found", lang));

    // 3. نرجع المنتج الذي تم إنشاؤه
    return Ok(ApiResponse.Success("تم إضافة المنتج بنجاح", "Product created successfully", lang, product));
}
    }
    
}
