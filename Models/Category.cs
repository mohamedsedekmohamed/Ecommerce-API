namespace EcommerceAPI.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // علاقة: التصنيف الواحد يحتوي على عدة منتجات (1-to-Many)
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}