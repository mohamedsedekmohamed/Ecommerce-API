using EcommerceAPI.Data;
using EcommerceAPI.DTOs.Auth;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;

        public AddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. جلب جميع عناوين المستخدم الحالي
        public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(string userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    Street = a.Street,
                    City = a.City,
                    State = a.State,
                    PhoneNumber = a.PhoneNumber,
                    IsDefault = a.IsDefault,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,


                })
                .OrderByDescending(a => a.IsDefault) // عرض العنوان الافتراضي أولاً
                .ToListAsync();
        }

        // 2. إضافة عنوان جديد
        public async Task<bool> AddAddressAsync(string userId, AddressDto model)
        {
            // تحقق إذا كان لدى المستخدم أي عناوين سابقة
            bool hasAnyAddress = await _context.Addresses.AnyAsync(a => a.UserId == userId);

            // إذا كان هذا أول عنوان، أو إذا اختار المستخدم جعله افتراضياً
            bool shouldBeDefault = !hasAnyAddress || model.IsDefault;

            // إذا كان العنوان الجديد سيكون افتراضياً، يجب إلغاء "الافتراضي" من العناوين الأخرى
            if (shouldBeDefault && hasAnyAddress)
            {
                await ResetDefaultAddresses(userId);
            }

            var newAddress = new Address
            {
                UserId = userId,
                Street = model.Street,
                City = model.City,
                State = model.State,
                PhoneNumber = model.PhoneNumber,
                IsDefault = shouldBeDefault,
                 Latitude = model.Latitude,
                    Longitude = model.Longitude,
            };

            await _context.Addresses.AddAsync(newAddress);
            return await _context.SaveChangesAsync() > 0;
        }

        // 3. تعيين عنوان محدد كعنوان افتراضي
        public async Task<bool> SetDefaultAddressAsync(string userId, int addressId)
        {
            var userAddresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var targetAddress = userAddresses.FirstOrDefault(a => a.Id == addressId);
            
            if (targetAddress == null) return false;

            // تحديث جميع العناوين: المختار يصبح True والباقي False
            foreach (var addr in userAddresses)
            {
                addr.IsDefault = (addr.Id == addressId);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // 4. حذف عنوان
        public async Task<bool> DeleteAddressAsync(string userId, int addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null) return false;

            bool wasDefault = address.IsDefault;

            _context.Addresses.Remove(address);
            var result = await _context.SaveChangesAsync() > 0;

            // إذا حذفنا العنوان الافتراضي وكان هناك عناوين أخرى، نجعل أول عنوان متاح هو الافتراضي
            if (result && wasDefault)
            {
                var nextAddress = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId);
                
                if (nextAddress != null)
                {
                    nextAddress.IsDefault = true;
                    await _context.SaveChangesAsync();
                }
            }

            return result;
        }

        // وظيفة مساعدة لإلغاء حالة "الافتراضي" من جميع عناوين المستخدم
        private async Task ResetDefaultAddresses(string userId)
        {
            var currentDefaults = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var addr in currentDefaults)
            {
                addr.IsDefault = false;
            }
        }
    }
}