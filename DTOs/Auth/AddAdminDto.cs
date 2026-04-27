using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Auth
{
    public class AddAdminDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; }
        // لتحديد هل هو SuperAdmin أم Admin
        [Required]
        [RegularExpression("Admin|SuperAdmin")]
        public string Role { get; set; } = string.Empty; 
    }
}