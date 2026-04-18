using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // تأمين الكنترولر: لا يمكن الدخول هنا بدون توكن (Token)
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // دالة مساعدة لجلب لغة الاستجابة
        private string GetLang() => Request.Headers["Accept-Language"].ToString();

        // ==========================================
        // 1. جلب جميع عناوين المستخدم
        // ==========================================
        [HttpGet("my-addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            var lang = GetLang();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(ApiResponse.Error("غير مصرح", "Unauthorized", lang));

            var addresses = await _addressService.GetUserAddressesAsync(userId);

            return Ok(ApiResponse.Success(
                "تم جلب العناوين بنجاح", 
                "Addresses fetched successfully", 
                lang, 
                addresses));
        }

        // ==========================================
        // 2. إضافة عنوان جديد
        // ==========================================
        [HttpPost("add")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDto model)
        {
            var lang = GetLang();

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Error("بيانات العنوان غير صحيحة", "Invalid address data", lang));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
                return Unauthorized(ApiResponse.Error("غير مصرح", "Unauthorized", lang));

            var result = await _addressService.AddAddressAsync(userId, model);

            if (!result)
                return BadRequest(ApiResponse.Error(
                    "حدث خطأ أثناء إضافة العنوان", 
                    "Error occurred while adding address", 
                    lang));

            return Ok(ApiResponse.Success(
                "تم إضافة العنوان بنجاح", 
                "Address added successfully", 
                lang));
        }

        // ==========================================
        // 3. تعيين عنوان كافتراضي (للتوصيل)
        // ==========================================
        [HttpPut("set-default/{addressId}")]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            var lang = GetLang();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(ApiResponse.Error("غير مصرح", "Unauthorized", lang));

            var result = await _addressService.SetDefaultAddressAsync(userId, addressId);

            if (!result)
                return NotFound(ApiResponse.Error(
                    "العنوان غير موجود أو لا تملك صلاحية تعديله", 
                    "Address not found or unauthorized", 
                    lang));

            return Ok(ApiResponse.Success(
                "تم تعيين العنوان كافتراضي بنجاح", 
                "Address set as default successfully", 
                lang));
        }

        // ==========================================
        // 4. حذف عنوان
        // ==========================================
        [HttpDelete("delete/{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var lang = GetLang();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(ApiResponse.Error("غير مصرح", "Unauthorized", lang));

            var result = await _addressService.DeleteAddressAsync(userId, addressId);

            if (!result)
                return NotFound(ApiResponse.Error(
                    "العنوان غير موجود أو لا تملك صلاحية حذفه", 
                    "Address not found or unauthorized", 
                    lang));

            return Ok(ApiResponse.Success(
                "تم حذف العنوان بنجاح", 
                "Address deleted successfully", 
                lang));
        }
    }
}