using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
[ApiController][Route("api/[controller]")]
public class BannersController : ControllerBase
{
    // نستخدم اسم _bannerService بدلاً من _context ليكون الكود واضحاً
    private readonly IBannerService _bannerService;

    public BannersController(IBannerService bannerService)
    {
        _bannerService = bannerService;
    }

    // --- عمليات المستخدم العادي ---

    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveBanner()
    {
        // استدعاء الدالة من الـ Service
        var banner = await _bannerService.GetActiveBannerAsync();

        if (banner == null) return NotFound("No active banner found.");

        return Ok(banner);
    }

    // --- عمليات الـ SuperAdmin ---

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet] 
    public async Task<ActionResult<IEnumerable<Banner>>> GetAllBanners()
    {
        // استدعاء الدالة من الـ Service
        var banners = await _bannerService.GetAllBannersAsync();
        return Ok(banners);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<Banner>> CreateBanner(Banner banner)
    {
        // استدعاء الدالة من الـ Service
        var createdBanner = await _bannerService.CreateBannerAsync(banner);
        return CreatedAtAction(nameof(GetActiveBanner), new { id = createdBanner.Id }, createdBanner);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBanner(int id, Banner banner)
    {
        // تنفيذ المنطق داخل الـ Service
        var updated = await _bannerService.UpdateBannerAsync(id, banner);
        
        if (!updated) return BadRequest("Update failed or ID mismatch.");

        return NoContent();
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBanner(int id)
    {
        // تنفيذ المسح داخل الـ Service
        var deleted = await _bannerService.DeleteBannerAsync(id);
        
        if (!deleted) return NotFound();

        return NoContent();
    }
}