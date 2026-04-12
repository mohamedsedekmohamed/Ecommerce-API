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
        public  required string Password { get; set; }
    }
}