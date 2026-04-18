public interface IBannerService
{
    Task<IEnumerable<Banner>> GetAllBannersAsync();
    Task<Banner?> GetActiveBannerAsync();
    Task<Banner?> GetByIdAsync(int id);
    Task<Banner> CreateBannerAsync(Banner banner);
    Task<bool> UpdateBannerAsync(int id, Banner banner);
    Task<bool> DeleteBannerAsync(int id);
}