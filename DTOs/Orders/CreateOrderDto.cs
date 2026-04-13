using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Orders
{
    public class CreateOrderDto
    {
  
[Required(ErrorMessage = "عنوان التوصيل مطلوب")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required, MinLength(1, ErrorMessage = "الطلب يجب أن يحتوي على منتج واحد على الأقل")]
        public List<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();
    }
}