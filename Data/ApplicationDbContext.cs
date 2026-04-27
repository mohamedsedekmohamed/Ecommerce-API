using EcommerceAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Data
{
    // نرث من IdentityDbContext وليس DbContext العادي لأننا نستخدم نظام المستخدمين الجاهز (Identity)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // مهم جداً لو بتستخدم Identity

            // منع الحذف التلقائي المتضارب (Cascade Delete) للمُرسل
            builder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // منع الحذف التلقائي المتضارب (Cascade Delete) للمُستقبل
            builder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
      
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; } 
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Banner> Banners { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        
    }
}