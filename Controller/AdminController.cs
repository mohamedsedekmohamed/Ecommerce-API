using EcommerceAPI.Constants;
using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يجب أن يكون المستخدم مسجلاً للدخول للوصول لأي مسار هنا
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        // 1. إضافة أدمن جديد (مسموح فقط للـ SuperAdmin)
        [HttpPost("add-admin")]
        [Authorize(Roles = AppRoles.SuperAdmin)] 
        public async Task<IActionResult> AddAdmin([FromBody] AddAdminDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.AddAdminAsync(model);
            if (!result.IsAuthenticated) return BadRequest(result.Message);

            return Ok(result);
        }

 // 2. تعديل بيانات وباسورد أدمن آخر (مسموح فقط للـ SuperAdmin)
        [HttpPut("update-admin/{adminId}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpdateAdmin(string adminId, [FromBody] UpdateAdminDto model) // 👈 استخدمنا DTO الجديد هنا
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 👈 استدعينا الدالة الجديدة
            var result = await _authService.UpdateAdminBySuperAdminAsync(adminId, model); 
            
            if (!result) return BadRequest("حدث خطأ أثناء تحديث بيانات الأدمن.");

            return Ok(new { Message = "تم تحديث بيانات الأدمن بنجاح." });
        }
     

        // 4. مسح الحساب الشخصي لنفسه
        [HttpDelete("delete-my-account")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return Unauthorized();

            var result = await _authService.DeleteUserAsync(currentUserId);
            if (!result) return BadRequest("حدث خطأ أثناء محاولة مسح الحساب.");

            return Ok(new { Message = "تم مسح حسابك بنجاح." });
        }
   // 3. تعديل بيانات الحساب الشخصي لنفسه (متاح لأي شخص مسجل الدخول)
     // مسار واحد لتعديل الحساب الشخصي (الاسم، الإيميل، وكلمة المرور إذا أراد)
        [HttpPut("update-my-profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return Unauthorized();

            // 1. تحديث البيانات الأساسية (الاسم والإيميل)
            // قمنا بتجهيز الـ DTO القديم الذي تعتمده الـ Service حالياً
            var updateBasicInfoDto = new UpdateUserDto 
            { 
                Name = model.Name, 
                Email = model.Email 
            };
            
            var profileResult = await _authService.UpdateUserAsync(currentUserId, updateBasicInfoDto);
            if (!profileResult) return BadRequest("حدث خطأ أثناء تحديث البيانات الأساسية.");

            // 2. تحديث كلمة المرور (فقط إذا قام بكتابة الباسورد القديم والجديد)
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var changePassDto = new ChangePasswordDto 
                { 
                    CurrentPassword = model.CurrentPassword, 
                    NewPassword = model.NewPassword 
                };
                
                var passResult = await _authService.ChangePasswordAsync(currentUserId, changePassDto);
                
                // لو البيانات اتعدلت بس الباسورد القديم كان غلط
                if (!passResult) 
                    return BadRequest("تم تحديث بياناتك (الاسم والإيميل)، ولكن فشل تغيير كلمة المرور لأن كلمة المرور الحالية غير صحيحة.");
            }

            return Ok(new { Message = "تم تحديث بيانات حسابك بنجاح." });
        }
        // 👇 الدوال الجديدة للـ SuperAdmin 👇

        // 6. عرض كل المديرين (Admins)
        [HttpGet("all-admins")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _authService.GetUsersByRoleAsync(AppRoles.Admin);
            return Ok(admins);
        }

        // 7. عرض كل المستخدمين العاديين (Users)
        [HttpGet("all-users")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetUsersByRoleAsync(AppRoles.User);
            return Ok(users);
        }

        [HttpDelete("delete-user/{userId}")]
        [Authorize(Roles = AppRoles.SuperAdmin)] // 👈 السوبر أدمن فقط هو من يملك هذه الصلاحية المطلقة
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _authService.DeleteUserAsync(userId);
            
            if (!result) return NotFound("المستخدم غير موجود أو حدث خطأ أثناء الحذف.");

            return Ok(new { Message = "تم مسح الحساب بنجاح." });
        }
    }
}