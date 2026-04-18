using System.ComponentModel.DataAnnotations;
public class Banner
{
    public int Id { get; set; }

    [Required]
    public string TitleAr { get; set; } = string.Empty;
    [Required]
    public string TitleEn { get; set; } = string.Empty;

    [Required]
    public string DescriptionAr { get; set; } = string.Empty;
    [Required]
    public string DescriptionEn { get; set; } = string.Empty;

    // أضف هذه الأسطر:
    public bool IsActive { get; set; } = true; // لتحديد الحالة
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // لتاريخ الإنشاء
}