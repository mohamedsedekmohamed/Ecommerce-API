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

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // 1. تسجيل مستخدم عادي (تم التأكد من إضافة الأرقام)
        public async Task<AuthModel> RegisterAsync(RegisterDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return new AuthModel { ErrorCode = "EmailExists" };

            var user = new ApplicationUser
            {
                UserName = model.Email, // يفضل أن يكون اليوزرنيم هو الإيميل لضمان التفرد
                Email = model.Email,
                FullName = model.FullName,
               PhoneNumber = model.PhoneNumber, // 👈 إضافة رقم الهاتف
                WhatsAppNumber = model.WhatsAppNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errorCode = result.Errors.First().Code;
                return new AuthModel { ErrorCode = errorCode };
            }

            await _userManager.AddToRoleAsync(user, AppRoles.User);
            
            return new AuthModel { Message = "تم إنشاء الحساب بنجاح!", IsAuthenticated = true, Email = user.Email };
        }

        // 2. تسجيل الدخول (كما هو)
        public async Task<AuthModel> LoginAsync(LoginDto model, string requiredRole) 
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthModel { ErrorCode = "InvalidCredentials" };

            var hasRole = await _userManager.IsInRoleAsync(user, requiredRole);
            if (!hasRole)
                return new AuthModel { ErrorCode = "UnauthorizedRole" };

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email!)
            };

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
                Id = user.Id,        
                Name = user.FullName,
                Email = user.Email,    
                Roles = userRoles.ToList()
            };
        }

        // 3. إضافة أدمن جديد (تم إضافة رقم الهاتف هنا)
        public async Task<AuthModel> AddAdminAsync(AddAdminDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return new AuthModel { ErrorCode = "EmailExists" };

            var user = new ApplicationUser
            {
                UserName = model.Email, // توحيد اليوزرنيم مع الإيميل
                Email = model.Email,
                FullName = model.Name,
                PhoneNumber = model.PhoneNumber // 👈 إضافة الرقم عند إنشاء الأدمن
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errorCode = result.Errors.First().Code;
                return new AuthModel { ErrorCode = errorCode };
            }

            var role = model.Role == AppRoles.SuperAdmin ? AppRoles.SuperAdmin : AppRoles.Admin;
            await _userManager.AddToRoleAsync(user, role);

            return new AuthModel { IsAuthenticated = true, Email = user.Email };
        }

        // 4. تحديث بيانات الحساب (تم إضافة تحديث رقم الهاتف)
        public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.FullName = model.Name;
            user.PhoneNumber = model.PhoneNumber; // 👈 تحديث رقم الهاتف

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            if (user.Email != model.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded) return false;

                var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                if (!setUserNameResult.Succeeded) return false;
            }

            return true;
        }

        // 5. جلب المستخدمين (إضافة رقم الهاتف للعرض)
        public async Task<IEnumerable<UserDetailsDto>> GetUsersByRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            
            return users.Select(u => new UserDetailsDto
            {
                Id = u.Id,
                Name = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty // 👈 عرض الرقم في القائمة
            });
        }

        // 6. تعديل أدمن بواسطة سوبر أدمن (إضافة الهاتف)
        public async Task<bool> UpdateAdminBySuperAdminAsync(string adminId, UpdateAdminDto model)
        {
            var user = await _userManager.FindByIdAsync(adminId);
            if (user == null) return false;

            user.FullName = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber; // 👈 إضافة تحديث الهاتف هنا أيضاً

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            if (!string.IsNullOrEmpty(model.Password))
            {
                var removePassResult = await _userManager.RemovePasswordAsync(user);
                if (removePassResult.Succeeded)
                {
                    await _userManager.AddPasswordAsync(user, model.Password);
                }
            }

            return true;
        }

        // 7. جلب كل الأدمن (إضافة الهاتف للعرض)
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
                PhoneNumber = user.PhoneNumber ?? "", // 👈 عرض رقم الهاتف للأدمن
                Roles = _userManager.GetRolesAsync(user).Result.ToList()
            });
        }

        // دوال الحذف وتغيير الباسورد تبقى كما هي...
        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}