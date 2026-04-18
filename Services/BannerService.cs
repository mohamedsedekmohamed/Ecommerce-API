using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;
public class BannerService : IBannerService
{
    private readonly ApplicationDbContext _context;

    public BannerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Banner>> GetAllBannersAsync()
    {
        return await _context.Banners.ToListAsync();
    }

    public async Task<Banner?> GetActiveBannerAsync()
    {
        return await _context.Banners
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Banner?> GetByIdAsync(int id)
    {
        return await _context.Banners.FindAsync(id);
    }

    public async Task<Banner> CreateBannerAsync(Banner banner)
    {
        _context.Banners.Add(banner);
        await _context.SaveChangesAsync();
        return banner;
    }

    public async Task<bool> UpdateBannerAsync(int id, Banner banner)
    {
        if (id != banner.Id) return false;

        _context.Entry(banner).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Banners.Any(e => e.Id == id)) return false;
            throw;
        }
    }

    public async Task<bool> DeleteBannerAsync(int id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null) return false;

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync();
        return true;
    }
}