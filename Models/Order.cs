using System.ComponentModel.DataAnnotations.Schema; // 👈 تمت إضافة هذه المكتبة
using EcommerceAPI.Enums;

namespace EcommerceAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")] // 👈 تمت إضافة هذا السطر لتحديد دقة الأرقام العشرية
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Preparing;

        // علاقة: الطلب ينتمي لمستخدم واحد
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // علاقة: الطلب يحتوي على عدة عناصر (منتجات داخل الطلب)
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}