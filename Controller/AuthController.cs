using EcommerceAPI.Constants;
using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private string GetLang() =>
            Request.Headers["Accept-Language"].ToString();

        // تسجيل دخول User
        [HttpPost("login/user")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto model)
        {
            return await ProcessLoginAsync(model, AppRoles.User);
        }

        // تسجيل دخول Admin
        [HttpPost("login/admin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginDto model)
        {
            return await ProcessLoginAsync(model, AppRoles.Admin);
        }

        // تسجيل دخول SuperAdmin
        [HttpPost("login/superadmin")]
        public async Task<IActionResult> LoginSuperAdmin([FromBody] LoginDto model)
        {
            return await ProcessLoginAsync(model, AppRoles.SuperAdmin);
        }

        // 🔥 DRY Login Method
      private async Task<IActionResult> ProcessLoginAsync(LoginDto model, string role)
{
    var lang = GetLang();

    if (!ModelState.IsValid)
        return BadRequest(ApiResponse.Error("بيانات غير صحيحة", "Invalid data", lang));

    var result = await _authService.LoginAsync(model, role);

    if (!result.IsAuthenticated)
        return BadRequest(ApiResponse.Error(
            result.Message, // ✅ سيعرض لك ما إذا كان الخطأ في الباسورد أم في نقص الصلاحيات
            "Login failed", 
            lang));

    return Ok(ApiResponse.Success(
        "تم تسجيل الدخول بنجاح",
        "Login successful",
        lang,
        new { token = result.Token, expiresOn = result.ExpiresOn }
    ));
}
        // تسجيل مستخدم جديد
       [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto model)
{
    var lang = GetLang();

    if (!ModelState.IsValid)
        return BadRequest(ApiResponse.Error("بيانات غير صحيحة", "Invalid data", lang));

    var result = await _authService.RegisterAsync(model);

    if (!result.IsAuthenticated)
        return BadRequest(ApiResponse.Error(
            result.Message, // ✅ استخدام الرسالة الحقيقية من السيرفيس (كالباسورد الضعيف)
            "Registration failed", 
            lang));

    return Ok(ApiResponse.Success("تم إنشاء الحساب بنجاح", "Account created successfully", lang));
}


    }
}