namespace EcommerceAPI.DTOs.Categories
{
    public class CategorySelectDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string NameAR { get; set; } = string.Empty;
                public string Icon { get; set; } = string.Empty;

            public int ProductsCount { get; set; } 
    }
}