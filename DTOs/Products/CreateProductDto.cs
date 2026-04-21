using System.ComponentModel.DataAnnotations;
using EcommerceAPI.Enums;

namespace EcommerceAPI.DTOs.Products
{
  public class CreateProductDto
{
    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    public string Name { get; set; } = string.Empty;

    public string NameAR { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
public Currency Currency { get; set; }
    public string DescriptionAR { get; set; } = string.Empty;

    [Required, Range(0.1, 100000)]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    [Required, Range(0, 10000)]
    public int Stock { get; set; }

    [Required]
    public int CategoryId { get; set; }
}
}