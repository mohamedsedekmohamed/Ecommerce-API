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
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        private string GetLang() =>
            Request.Headers["Accept-Language"].ToString();
[HttpGet("admins")]
[Authorize(Roles = AppRoles.SuperAdmin)]
public async Task<IActionResult> GetAdmins()
{
    var lang = GetLang();

    var admins = await _authService.GetAdminsAsync();

    return Ok(ApiResponse.Success( "تم جلب المدراء بنجاح", "Admins fetched successfully", lang,admins));
}
[HttpPut("update-superadmin-profile")]
[Authorize(Roles = AppRoles.SuperAdmin)]
public async Task<IActionResult> UpdateSuperAdminProfile([FromBody] UpdateMyProfileDto model)
{
    var lang = GetLang();

    if (!ModelState.IsValid)
        return BadRequest(ApiResponse.Error("بيانات غير صحيحة", "Invalid data", lang));

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null)
        return Unauthorized(ApiResponse.Error("غير مصرح", "Unauthorized", lang));

    // 1. تحديث الاسم والإيميل
    var updateDto = new UpdateUserDto
    {
        Name = model.Name,
        Email = model.Email
    };

    var result = await _authService.UpdateUserAsync(userId, updateDto);

    if (!result)
        return BadRequest(ApiResponse.Error(
            "فشل تحديث البيانات",
            "Failed to update profile",
            lang));

    // 2. 🔥 الجزء اللي كان مفقود: تحديث الباسورد
    if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
    {
        var passResult = await _authService.ChangePasswordAsync(userId,
            new ChangePasswordDto
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword
            });

        // لو الباسورد القديم غلط أو الجديد ضعيف
        if (!passResult) 
            return BadRequest(ApiResponse.Error(
                "تم تحديث الاسم والإيميل، لكن كلمة المرور القديمة غير صحيحة أو الجديدة ضعيفة",
                "Profile updated but password change failed",
                lang));
    }

    return Ok(ApiResponse.Success(
        "تم تحديث بيانات السوبر أدمن وتغيير كلمة المرور بنجاح",
        "SuperAdmin profile and password updated",
        lang));
}








        // 1. إضافة أدمن
        [HttpPost("add-admin")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> AddAdmin([FromBody] AddAdminDto model)
        {
            var lang = GetLang();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error("بيانات غير صحيحة", "Invalid data", lang));

            var result = await _authService.AddAdminAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(ApiResponse.Error(result.Message, result.Message, lang));

            return Ok(ApiResponse.Success("تم إضافة الأدمن بنجاح", "Admin added successfully", lang));
        }

        // 2. تعديل أدمن
        [HttpPut("update-admin/{adminId}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpdateAdmin(string adminId, [FromBody] UpdateAdminDto model)
        {
            var lang = GetLang();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error("بيانات غير صحيحة", "Invalid data", lang));

            var result = await _authService.UpdateAdminBySuperAdminAsync(adminId, model);

            if (!result)
                return BadRequest(ApiResponse.Error(
                    "حدث خطأ أثناء تحديث بيانات الأدمن",
                    "Error updating admin",
                    lang));

            return Ok(ApiResponse.Success(
                "تم تحديث بيانات الأدمن بنجاح",
                "Admin updated successfully",
                lang));
        }

        // حذف حسابي
       // 1. حذف حسابي الشخصي (لأي مستخدم مسجل دخول)
       // ==========================================
        // 1. حذف حسابي - خاص بالمستخدم العادي
        // ==========================================
        [HttpDelete("delete-my-account-user")]
        [Authorize(Roles = AppRoles.User)]
        public async Task<IActionResult> DeleteMyAccountUser()
        {
            var lang = GetLang();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // هنا ممكن تستدعي دالة من السيرفيس مخصصة لحذف المستخدم العادي
            var result = await _authService.DeleteUserAsync(userId); 

            if (!result) return BadRequest(ApiResponse.Error("خطأ أثناء الحذف", "Error", lang));
            return Ok(ApiResponse.Success("تم حذف حسابك (كمستخدم) بنجاح", "User account deleted", lang));
        }

        // ==========================================
        // 2. حذف حسابي - خاص بالأدمن
        // ==========================================
        [HttpDelete("delete-my-account-admin")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> DeleteMyAccountAdmin()
        {
            var lang = GetLang();
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // هنا ممكن تستدعي دالة مخصصة للأدمن (مثلاً بتنقل منتجاته قبل الحذف)
            var result = await _authService.DeleteUserAsync(adminId); 

            if (!result) return BadRequest(ApiResponse.Error("خطأ أثناء الحذف", "Error", lang));
            return Ok(ApiResponse.Success("تم حذف حسابك (كأدمن) بنجاح", "Admin account deleted", lang));
        }

        // ==========================================
        // 3. حذف حسابي - خاص بالسوبر أدمن
        // ==========================================
        [HttpDelete("delete-my-account-superadmin")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteMyAccountSuperAdmin()
        {
            var lang = GetLang();
            var superAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // ⚠️ تنبيه: يفضل تتأكد هنا إن ده مش آخر سوبر أدمن في الداتابيز عشان السيستم ميقفلش!
            var result = await _authService.DeleteUserAsync(superAdminId); 

            if (!result) return BadRequest(ApiResponse.Error("خطأ أثناء الحذف", "Error", lang));
            return Ok(ApiResponse.Success("تم حذف حسابك (كسوبر أدمن) بنجاح", "SuperAdmin account deleted", lang));
        }
        // 3. حذف أدمن
        [HttpDelete("delete-admin/{adminId}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteAdmin(string adminId)
        {
            var lang = GetLang();

            // اختياري: منع السوبر أدمن من حذف نفسه بالخطأ من هذا الـ Endpoint
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == adminId)
                return BadRequest(ApiResponse.Error(
                    "لا يمكنك حذف حسابك الشخصي من هنا", 
                    "You cannot delete your own account from here", 
                    lang));

            var result = await _authService.DeleteUserAsync(adminId);

            if (!result)
                return NotFound(ApiResponse.Error(
                    "حساب الأدمن غير موجود أو حدث خطأ أثناء الحذف",
                    "Admin not found or error occurred during deletion",
                    lang));

            return Ok(ApiResponse.Success(
                "تم حذف حساب الأدمن بنجاح",
                "Admin deleted successfully",
                lang));
        }
 [HttpGet("all-users")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetUsersByRoleAsync(AppRoles.User);
            return Ok(users);
        }
        // تعديل حسابي
        [HttpPut("update-my-profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileDto model)
        {
            var lang = GetLang();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error("بيانات غير صحيحة", "Invalid data", lang));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(ApiResponse.Error("غير مصرح", "Unauthorized", lang));

            var updateDto = new UpdateUserDto
            {
                Name = model.Name,
                Email = model.Email
            };

            var result = await _authService.UpdateUserAsync(userId, updateDto);

            if (!result)
                return BadRequest(ApiResponse.Error(
                    "فشل تحديث البيانات",
                    "Failed to update profile",
                    lang));

            if (!string.IsNullOrEmpty(model.CurrentPassword) &&
                !string.IsNullOrEmpty(model.NewPassword))
            {
                var passResult = await _authService.ChangePasswordAsync(userId,
                    new ChangePasswordDto
                    {
                        CurrentPassword = model.CurrentPassword,
                        NewPassword = model.NewPassword
                    });

                if (!passResult)
                    return BadRequest(ApiResponse.Error(
                        "تم تحديث البيانات لكن كلمة المرور غير صحيحة",
                        "Profile updated but password is incorrect",
                        lang));
            }

            return Ok(ApiResponse.Success(
                "تم تحديث الحساب بنجاح",
                "Profile updated successfully",
                lang));
        }

        // حذف مستخدم
        [HttpDelete("delete-user/{userId}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var lang = GetLang();

            var result = await _authService.DeleteUserAsync(userId);

            if (!result)
                return NotFound(ApiResponse.Error(
                    "المستخدم غير موجود",
                    "User not found",
                    lang));

            return Ok(ApiResponse.Success(
                "تم حذف الحساب بنجاح",
                "User deleted successfully",
                lang));
        }

        // 1. حذف حسابي الشخصي (لأي مستخدم مسجل دخول)
      
    }
}