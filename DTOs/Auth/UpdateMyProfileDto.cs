namespace EcommerceAPI.DTOs.Auth
{
    public class UpdateMyProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // جعلناها اختيارية (Nullable) حتى لا نُجبره على تغيير الباسورد في كل مرة
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}