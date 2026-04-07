using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Orders
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "يجب تحديد رقم المستخدم")]
        public string UserId { get; set; } = string.Empty; 

        [Required, MinLength(1, ErrorMessage = "الطلب يجب أن يحتوي على منتج واحد على الأقل")]
        public List<OrderItemRequestDto> Items { get; set; } = new List<OrderItemRequestDto>();
    }
}