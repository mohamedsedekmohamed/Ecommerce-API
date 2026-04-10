using EcommerceAPI.Constants;
using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        // مسار تسجيل دخول المستخدم العادي
        // POST: api/auth/login/user
        [HttpPost("login/user")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto model)
        {
            return await ProcessLoginAsync(model, AppRoles.User);
        }

        // مسار تسجيل دخول الأدمن
        // POST: api/auth/login/admin
        [HttpPost("login/admin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginDto model)
        {
            return await ProcessLoginAsync(model, AppRoles.Admin);
        }

        // مسار تسجيل دخول السوبر أدمن
        // POST: api/auth/login/superadmin
        [HttpPost("login/superadmin")]
        public async Task<IActionResult> LoginSuperAdmin([FromBody] LoginDto model)
        {
            return await ProcessLoginAsync(model, AppRoles.SuperAdmin);
        }

        // دالة مساعدة لمنع تكرار الكود (DRY Principle)
        private async Task<IActionResult> ProcessLoginAsync(LoginDto model, string requiredRole)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            // نمرر الدور المطلوب للـ Service للتحقق منه
            var result = await _authService.LoginAsync(model, requiredRole);
            
            if (!result.IsAuthenticated) 
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}