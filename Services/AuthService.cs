using EcommerceAPI.Constants;
using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        // عمل Injection للأدوات التي نحتاجها
        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // 1. تسجيل مستخدم عادي
        public async Task<AuthModel> RegisterAsync(RegisterDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return new AuthModel { Message = "الإيميل مسجل بالفعل!" };

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, AppRoles.User);
            
            return new AuthModel { Message = "تم إنشاء الحساب بنجاح!", IsAuthenticated = true, Email = user.Email };
        }

        // 2. تسجيل الدخول
    public async Task<AuthModel> LoginAsync(LoginDto model, string requiredRole) 
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthModel { Message = "الإيميل أو كلمة المرور غير صحيحة!" };

            // 👇 التعديل الثاني الهام: التحقق من أن هذا المستخدم يمتلك الصلاحية المطلوبة للدخول من هذا المسار
            var hasRole = await _userManager.IsInRoleAsync(user, requiredRole);
            if (!hasRole)
                return new AuthModel { Message = $"غير مصرح لك بالدخول كـ {requiredRole}!" };

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            // جلب صلاحيات المستخدم وإضافتها للـ Token 
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(14),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new AuthModel
            {
                Message = "تم تسجيل الدخول بنجاح!",
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresOn = token.ValidTo,
            };
        }

        // 3. إضافة أدمن جديد (خاص بالـ SuperAdmin)
       public async Task<AuthModel> AddAdminAsync(AddAdminDto model)
{
    if (await _userManager.FindByEmailAsync(model.Email) != null)
        return new AuthModel { Message = "الإيميل مسجل بالفعل!" };

    var user = new ApplicationUser
    {
        UserName = model.Email,
        Email = model.Email,
        FullName = model.Name
    };

    var result = await _userManager.CreateAsync(user, model.Password);

    if (!result.Succeeded)
    {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return new AuthModel { Message = errors };
    }

    // ✅ التحقق من الـ Role
    var role = model.Role == AppRoles.SuperAdmin 
        ? AppRoles.SuperAdmin 
        : AppRoles.Admin;

    await _userManager.AddToRoleAsync(user, role);

    return new AuthModel
    {
        Message = "تم إضافة الأدمن بنجاح!",
        IsAuthenticated = true,
        Email = user.Email
    };
}
        // 4. تعديل بيانات الحساب (الاسم والإيميل)
        public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.FullName = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email; 

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // 5. تغيير كلمة المرور
        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            return result.Succeeded;
        }

        // 6. مسح الحساب
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        // إضافة هذه الدالة داخل AuthService
        public async Task<IEnumerable<UserDetailsDto>> GetUsersByRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            
            return users.Select(u => new UserDetailsDto
            {
                Id = u.Id,
                Name = u.FullName ?? string.Empty, // تأكد من استخدام FullName كما هو معرف في الـ Model الخاص بك
                Email = u.Email ?? string.Empty
            });
        }




        // أضف هذا الكود داخل AuthService.cs
        public async Task<bool> UpdateAdminBySuperAdminAsync(string adminId, UpdateAdminDto model)
        {
            var user = await _userManager.FindByIdAsync(adminId);
            if (user == null) return false;

            // 1. تحديث البيانات الأساسية
            user.FullName = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email; 

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            // 2. تحديث الباسورد (فقط في حال قام السوبر أدمن بكتابة باسورد جديد)
            if (!string.IsNullOrEmpty(model.Password))
            {
                // مسح الباسورد القديم (بدون الحاجة لمعرفته)
                var removePassResult = await _userManager.RemovePasswordAsync(user);
                
                if (removePassResult.Succeeded)
                {
                    // إضافة الباسورد الجديد
                    await _userManager.AddPasswordAsync(user, model.Password);
                }
            }

            return true;
        }


        public async Task<IEnumerable<AdminDto>> GetAdminsAsync()
{
    var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
    var superAdmins = await _userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);

    var allAdmins = admins.Concat(superAdmins).Distinct();

    return allAdmins.Select(user => new AdminDto
    {
        Id = user.Id,
        Name = user.FullName ?? "",
        Email = user.Email ?? "",
        Roles = _userManager.GetRolesAsync(user).Result
    });
}
    }
}