using ECommerceAPI.Models;


namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IRolesRepository
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int roleId);
        Task<int> CreateRoleAsync(string roleName);
        Task<bool> UpdateRoleAsync(int roleId, string roleName);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<List<Rights>> GetRightsByRoleIdAsync(int roleId);
    }
}
