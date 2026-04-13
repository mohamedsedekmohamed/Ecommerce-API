namespace EcommerceAPI.DTOs.Orders
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        // 👇 الحقول الجديدة للإرجاع في الـ JSON
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        // 👇 تم التغيير هنا لاستخدام DTO الاستجابة الجديد
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }
}