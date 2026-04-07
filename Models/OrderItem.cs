using System.ComponentModel.DataAnnotations.Schema; // 👈 تمت إضافة هذه المكتبة

namespace EcommerceAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")] // 👈 تمت إضافة هذا السطر لتحديد دقة الأرقام العشرية
        public decimal UnitPrice { get; set; }

        // علاقة: العنصر ينتمي لطلب واحد
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // علاقة: العنصر عبارة عن منتج معين
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}