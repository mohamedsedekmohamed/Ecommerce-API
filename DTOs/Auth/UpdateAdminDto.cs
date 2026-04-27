namespace EcommerceAPI.DTOs.Auth
{
    public class UpdateAdminDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        // جعلناه اختيارياً (Nullable) لكي لا يكون إجبارياً في كل مرة يعدل فيها الاسم فقط
        public string? Password { get; set; } 
    }
}