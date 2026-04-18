using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
        [StringLength(100)]
        public  required string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تتكون من 6 أحرف على الأقل")]

        public string PhoneNumber { get; set; } = string.Empty;

        public string WhatsAppNumber { get; set; }= string.Empty; // يمكن جعله اختياري (Nullable)
        public  required string Password { get; set; }
    }
}