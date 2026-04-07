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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound($"التصنيف رقم {id} غير موجود.");
            return Ok(category);
        }

        // 👇 التعديل هنا: السوبر أدمن فقط هو من يقدر يضيف تصنيف جديد
        [HttpPost]
        [Authorize(Roles = AppRoles.SuperAdmin)] 
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await _categoryService.CreateCategoryAsync(dto);
            return Ok(category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)] 
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await _categoryService.UpdateCategoryAsync(id, dto);
            if (category == null) return NotFound($"التصنيف رقم {id} غير موجود.");

            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)] 
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _categoryService.DeleteCategoryAsync(id);
            if (!isDeleted) return NotFound($"التصنيف رقم {id} غير موجود.");

            return Ok("تم حذف التصنيف بنجاح.");
        }
    }
}