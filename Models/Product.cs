using System.ComponentModel.DataAnnotations.Schema;
using EcommerceAPI.Enums;

namespace EcommerceAPI.Models
{
    public class Product
    {
        public bool IsActive { get; set; } = true;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
public string NameAR { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
public string DescriptionAR { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }
public Currency Currency { get; set; } = Currency.USD;
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // 👈 الحقل الجديد لمعرفة من قام بإضافة المنتج
        public string? CreatedByUserId { get; set; } 
        [ForeignKey("CreatedByUserId")]
    public ApplicationUser CreatedByUser { get; set; }
    }
}