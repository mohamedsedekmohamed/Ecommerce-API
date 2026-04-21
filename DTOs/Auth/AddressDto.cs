using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Auth // أو يمكنك وضعه في مجلد DTOs/Users حسب ترتيب مشروعك
{
    public class AddressDto
    {
        // نستخدم الـ Id عند جلب العناوين أو عند تعديل/حذف عنوان محدد
        public int Id { get; set; } 

        [Required(ErrorMessage = "اسم الشارع أو الحي مطلوب")]
        public string Street { get; set; }

        [Required(ErrorMessage = "اسم المدينة مطلوب")]
        public string City { get; set; }

        public string State { get; set; } // المحافظة أو المنطقة
         
           public string Latitude { get; set; }
        public string Longitude { get; set; }

        [Required(ErrorMessage = "رقم الهاتف الخاص بهذا العنوان مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        public string PhoneNumber { get; set; }

        // إذا أرسل المستخدم true، سيتم تعيين هذا العنوان كالعنوان الأساسي
        public bool IsDefault { get; set; } 
    }
}