using ECommerceAPI.DTO_s;

namespace ECommerceAPI.Repositories
{
    public interface IAddressRepository
    {
        Task<IEnumerable<AddressDto>> GetByUserIdAsync(int userId);
        Task<AddressDto?> GetByIdAsync(int addressId, int userId);
        Task<int> CreateAsync(int userId, CreateAddressDto dto);
        Task<bool> UpdateAsync(int addressId, int userId, CreateAddressDto dto);
        Task<bool> DeleteAsync(int addressId, int userId);
        Task<bool> SetDefaultAsync(int addressId, int userId);
    }
}
