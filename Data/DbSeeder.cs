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

            // 2. إنشاء حساب المدير العام الافتراضي
            var superAdminEmail = "admin@ecommerce.com";
            var existingAdmin = await userManager.FindByEmailAsync(superAdminEmail);

            if (existingAdmin == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FullName = "Super Admin"
                };

                // الباسورد الافتراضي للمدير (يمكنك تغييره لاحقاً)
                var result = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    // إعطاء هذا الحساب صلاحية SuperAdmin
                    await userManager.AddToRoleAsync(newAdmin, AppRoles.SuperAdmin);
                }
            }
        }
    }
}