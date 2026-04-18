public class SummaryResponseDto
{
    public int TotalCategories { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; } 
    public int TotalUsers { get; set; } 
    public decimal TotalRevenue { get; set; } 
    public int PendingOrders { get; set; } 
    public int OutOfStockProducts { get; set; } 

    // --- تم التعديل هنا ---
    public string TopUserName { get; set; } // بدلاً من TopUserId
    public string TopProductName { get; set; } // بدلاً من TopProductId
}