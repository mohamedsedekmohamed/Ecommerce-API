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
// 🔥 DRY Login Method
private async Task<IActionResult> ProcessLoginAsync(LoginDto model, string role)
{
    var lang = GetLang();

    if (!ModelState.IsValid)
        return BadRequest(ApiResponse.Error("بيانات غير صحيحة", "Invalid data", lang));

    var result = await _authService.LoginAsync(model, role);

    if (!result.IsAuthenticated)
        return BadRequest(ApiResponse.Error(
            result.Message, 
            "Login failed", 
            lang));

    // ✅ إرجاع بيانات التوثيق وتفاصيل المستخدم بوضوح
    return Ok(ApiResponse.Success(
        "تم تسجيل الدخول بنجاح",
        "Login successful",
        lang,
        new 
        { 
            token = result.Token, 
            expiresOn = result.ExpiresOn,
            userDetails = new 
            {
                id = result.Id,
                email = result.Email,
                username = result.Username,
                roles = result.Roles
            }
        }
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
{
    var (ar, en) = result.ErrorCode switch
    {
        "EmailExists" => ("البريد الإلكتروني مستخدم بالفعل", "Email already exists"),
        "InvalidCredentials" => ("الإيميل أو كلمة المرور غير صحيحة", "Invalid email or password"),
        "UnauthorizedRole" => ("غير مصرح لك بالدخول بهذا الدور", "Unauthorized role"),
        
        // Identity Errors 👇
        "PasswordTooShort" => ("كلمة المرور قصيرة جداً", "Password is too short"),
        "PasswordRequiresDigit" => ("يجب أن تحتوي كلمة المرور على رقم", "Password must contain a digit"),
        "PasswordRequiresUpper" => ("يجب أن تحتوي على حرف كبير", "Password must contain uppercase letter"),
        "PasswordRequiresNonAlphanumeric" => ("يجب أن تحتوي على رمز", "Password must contain special character"),

        _ => ("حدث خطأ", "Something went wrong")
    };

    return BadRequest(ApiResponse.Error(ar, en, lang));
}

    return Ok(ApiResponse.Success("تم إنشاء الحساب بنجاح", "Account created successfully", lang));
}


    }
}