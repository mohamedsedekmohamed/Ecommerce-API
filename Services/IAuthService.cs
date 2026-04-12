using EcommerceAPI.DTOs.Auth;

namespace EcommerceAPI.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterDto model);

        Task<AuthModel> AddAdminAsync(AddAdminDto model);
        Task<bool> UpdateUserAsync(string userId, UpdateUserDto model);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto model);
        Task<IEnumerable<UserDetailsDto>> GetUsersByRoleAsync(string roleName);
        Task<bool> UpdateAdminBySuperAdminAsync(string adminId, UpdateAdminDto model);
        Task<AuthModel> LoginAsync(LoginDto model, string requiredRole);
        Task<IEnumerable<AdminDto>> GetAdminsAsync();
    }
}