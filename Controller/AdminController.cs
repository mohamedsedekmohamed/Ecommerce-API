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

    return Ok(ApiResponse.Success(
        "تم تحديث بيانات السوبر أدمن",
        "SuperAdmin profile updated",
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
        [HttpDelete("delete-my-account")]
        // مش محتاجين نحدد Role هنا لأن أي حد من حقه يحذف حسابه
        public async Task<IActionResult> DeleteMyAccount()
        {
            var lang = GetLang();

            // بنجيب الـ ID بتاع الشخص اللي عامل Request من التوكن بتاعه
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(ApiResponse.Error("غير مصرح", "Unauthorized", lang));

            var result = await _authService.DeleteUserAsync(userId);

            if (!result)
                return BadRequest(ApiResponse.Error(
                    "حدث خطأ أثناء حذف الحساب",
                    "Error deleting account",
                    lang));

            return Ok(ApiResponse.Success(
                "تم حذف حسابك بنجاح",
                "Account deleted successfully",
                lang));
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