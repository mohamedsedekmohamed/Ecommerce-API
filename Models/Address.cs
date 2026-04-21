namespace EcommerceAPI.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string UserId { get; set; } // ربط العنوان بالمستخدم
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDefault { get; set; } // لتحديد العنوان الأساسي
public string? Latitude { get; set; }  // خط العرض (Lat)
        public string? Longitude { get; set; } // خط الطول (Lng/Lon)
        // Navigation Property لربطها بـ IdentityUser
        public ApplicationUser User { get; set; }
    }
}