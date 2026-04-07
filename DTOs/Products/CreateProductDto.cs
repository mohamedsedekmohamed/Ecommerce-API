using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Products
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required, Range(0.1, 100000, ErrorMessage = "السعر يجب أن يكون أكبر من الصفر")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        [Required, Range(0, 10000, ErrorMessage = "المخزون لا يمكن أن يكون بالسالب")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "يجب تحديد رقم التصنيف")]
        public int CategoryId { get; set; }
    }
}