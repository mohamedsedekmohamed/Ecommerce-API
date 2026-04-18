using Microsoft.AspNetCore.Identity;

namespace EcommerceAPI.Models
{
  
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? WhatsAppNumber { get; set; }
    }
}