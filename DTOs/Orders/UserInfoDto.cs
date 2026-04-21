namespace EcommerceAPI.DTOs.Orders;

public class UserInfoDto 
{
    public string Id { get; set; }
    public string FullName { get; set; } // استبدلها باسم الخاصية عندك زي FirstName أو Name
    public string Email { get; set; }
    public string phoneNumber { get; set; }
    public string whatsAppNumber { get; set; }
    // تقدر تضيف أي بيانات تانية عايز ترجعها زي رقم التليفون الأساسي وغيرها
}