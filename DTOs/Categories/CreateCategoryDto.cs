using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Categories
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "اسم التصنيف مطلوب")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}