using EcommerceAPI.DTOs.Auth;

public interface IAddressService
{
    Task<IEnumerable<AddressDto>> GetUserAddressesAsync(string userId);
    Task<bool> AddAddressAsync(string userId, AddressDto model);
    Task<bool> DeleteAddressAsync(string userId, int addressId);
    Task<bool> SetDefaultAddressAsync(string userId, int addressId);
}