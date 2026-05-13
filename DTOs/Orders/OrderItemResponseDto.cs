namespace EcommerceAPI.DTOs.Orders
{
    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductNameAR { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorPhone { get; set; }
    }
}