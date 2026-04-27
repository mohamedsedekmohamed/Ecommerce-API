using EcommerceAPI.Constants;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace EcommerceAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndSuperAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. إنشاء الصلاحيات إذا لم تكن موجودة
            if (!await roleManager.RoleExistsAsync(AppRoles.SuperAdmin))
                await roleManager.CreateAsync(new IdentityRole(AppRoles.SuperAdmin));

            if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));

            if (!await roleManager.RoleExistsAsync(AppRoles.User))
                await roleManager.CreateAsync(new IdentityRole(AppRoles.User));

            // 2. تجهيز قائمة الحسابات الثلاثة الافتراضية
            var defaultUsers = new List<(ApplicationUser User, string Password, string Role)>
            {
                // حساب السوبر أدمن
                (new ApplicationUser { 
                    UserName = "mohamedsuperadmin", 
                    Email = "superadmin@ecommerce.com", 
                    FullName = "Super Admin" 
                }, "Admin@123", AppRoles.SuperAdmin),

                // حساب الأدمن
                (new ApplicationUser { 
                    UserName = "mohamedadmin", 
                    Email = "admin@ecommerce.com", 
                    FullName = "System Admin" 
                }, "Admin@123", AppRoles.Admin),

                // حساب المستخدم العادي
                (new ApplicationUser { 
                    UserName = "mohameduser", 
                    Email = "user@ecommerce.com", 
                    FullName = "Test User" 
                }, "User@123", AppRoles.User)
            };

            // 3. المرور على القائمة وإنشاء الحسابات إذا لم تكن موجودة
            foreach (var item in defaultUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(item.User.Email);

                if (existingUser == null)
                {
                    var result = await userManager.CreateAsync(item.User, item.Password);
                    if (result.Succeeded)
                    {
                        // إعطاء الصلاحية المناسبة لكل حساب
                        await userManager.AddToRoleAsync(item.User, item.Role);
                    }
                }
            }
        }
    }
}