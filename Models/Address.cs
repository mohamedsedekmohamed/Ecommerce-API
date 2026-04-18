namespace EcommerceAPI.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string UserId { get; set; } // ربط العنوان بالمستخدم
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDefault { get; set; } // لتحديد العنوان الأساسي

        // Navigation Property لربطها بـ IdentityUser
        public ApplicationUser User { get; set; }
    }
}