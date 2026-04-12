using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Categories
{
    public class CreateCategoryDto
    {
            public required string Name { get; set; }
    public required string NameAR { get; set; }

    public  required string Description { get; set; }
    public required string DescriptionAR { get; set; }
    }
}