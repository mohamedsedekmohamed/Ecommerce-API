namespace EcommerceAPI.DTOs.Auth
{
    public class AuthModel
    {
        public string Message { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
           public string? ErrorCode { get; set; } // 👈 جديد
public string? MessageAr { get; set; } // (اختياري للـ success)
public string? MessageEn { get; set; }
public string Name { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOn { get; set; }

        // 🔥 تفاصيل المستخدم التي سيتم إرجاعها
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>(); 
        // يمكنك إضافة أي بيانات أخرى هنا مثل PhoneNumber أو FullName
    }
}