using EcommerceAPI.DTOs.Categories;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Constants;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        private string GetLang() =>
            Request.Headers["Accept-Language"].ToString();

        // ==========================================
        // User
        // ==========================================
        [HttpGet("user/all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoriesForUser()
        {
            var lang = GetLang();

            var categories = await _categoryService.GetAllCategoriesAsync();

            return Ok(ApiResponse.Success(
                "تم جلب التصنيفات بنجاح",
                "Categories fetched successfully",
                lang,
                categories));
        }

        // ==========================================
        // Admin
        // ==========================================
        [HttpGet("admin/all")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> GetCategoriesForAdmin()
        {
            var lang = GetLang();

            var categories = await _categoryService.GetAllCategoriesAsync();

            return Ok(ApiResponse.Success(
                "تم جلب التصنيفات بنجاح",
                "Categories fetched successfully",
                lang,
                categories));
        }

        // ==========================================
        // SuperAdmin
        // ==========================================
        [HttpGet("superadmin/all")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetCategoriesForSuperAdmin()
        {
            var lang = GetLang();

            var categories = await _categoryService.GetAllCategoriesAsync();

            return Ok(ApiResponse.Success(
                "تم جلب التصنيفات بنجاح",
                "Categories fetched successfully",
                lang,
                categories));
        }

        // ==========================================
        // Get By Id
        // ==========================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var lang = GetLang();

            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound(ApiResponse.Error(
                    $"التصنيف رقم {id} غير موجود",
                    $"Category with id {id} not found",
                    lang));

            return Ok(ApiResponse.Success(
                "تم جلب التصنيف بنجاح",
                "Category fetched successfully",
                lang,
                category));
        }

        // ==========================================
        // Create
        // ==========================================
        [HttpPost]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var lang = GetLang();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error(
                    "بيانات غير صحيحة",
                    "Invalid data",
                    lang));

            var category = await _categoryService.CreateCategoryAsync(dto);

            return Ok(ApiResponse.Success(
                "تم إنشاء التصنيف بنجاح",
                "Category created successfully",
                lang,
                category));
        }

        // ==========================================
        // Update
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
        {
            var lang = GetLang();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error(
                    "بيانات غير صحيحة",
                    "Invalid data",
                    lang));

            var category = await _categoryService.UpdateCategoryAsync(id, dto);

            if (category == null)
                return NotFound(ApiResponse.Error(
                    $"التصنيف رقم {id} غير موجود",
                    $"Category with id {id} not found",
                    lang));

            return Ok(ApiResponse.Success(
                "تم تحديث التصنيف بنجاح",
                "Category updated successfully",
                lang,
                category));
        }

        // ==========================================
        // Delete
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var lang = GetLang();

            var isDeleted = await _categoryService.DeleteCategoryAsync(id);

            if (!isDeleted)
                return NotFound(ApiResponse.Error(
                    $"التصنيف رقم {id} غير موجود",
                    $"Category with id {id} not found",
                    lang));

            return Ok(ApiResponse.Success(
                "تم حذف التصنيف بنجاح",
                "Category deleted successfully",
                lang));
        }
    }
}