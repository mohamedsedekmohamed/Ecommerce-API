using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Orders
{
    public class OrderItemRequestDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required, Range(1, 100, ErrorMessage = "الكمية يجب أن تكون بين 1 و 100")]
        public int Quantity { get; set; }
    }
}