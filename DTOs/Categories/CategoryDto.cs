namespace EcommerceAPI.DTOs.Categories
{
    public class CategoryDto
    {
        public int Id { get; set; }
       public required string Name { get; set; }
    public required string NameAR { get; set; }

    public required string Description { get; set; }
    public required string Icon { get; set; }
    public required string DescriptionAR { get; set; }
    }
}