using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Base;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories
{
    public class RolesRepository : BaseRepository, IRolesRepository
    {
        public RolesRepository(DatabaseHelper db) : base(db) { }

        // ✅ All Roles తీసుకురా
        public async Task<List<Role>> GetAllRolesAsync()
        {
            const string sql = "SELECT RoleId, RoleName FROM Roles";
            return await ExecuteQueryAsync(sql, null, r => new Role
            {
                RoleId = (int)r["RoleId"],
                RoleName = r["RoleName"].ToString()!
            });
        }

        // ✅ Single Role తీసుకురా
        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            const string sql = "SELECT RoleId, RoleName FROM Roles WHERE RoleId = @RoleId";
            var ps = new[] { new SqlParameter("@RoleId", roleId) };
            var result = await ExecuteQueryAsync(sql, ps, r => new Role
            {
                RoleId = (int)r["RoleId"],
                RoleName = r["RoleName"].ToString()!
            });
            return result.FirstOrDefault();
        }

        // ✅ New Role create చేయి
        public async Task<int> CreateRoleAsync(string roleName)
        {
            const string sql = @"INSERT INTO Roles (RoleName) 
                                 VALUES (@RoleName); 
                                 SELECT SCOPE_IDENTITY();";
            var ps = new[] { new SqlParameter("@RoleName", roleName) };
            var result = await ExecuteScalarAsync(sql, ps);
            return Convert.ToInt32(result);
        }

        // ✅ Role update చేయి
        public async Task<bool> UpdateRoleAsync(int roleId, string roleName)
        {
            const string sql = @"UPDATE Roles SET RoleName = @RoleName 
                                 WHERE RoleId = @RoleId";
            var ps = new[]
            {
                new SqlParameter("@RoleName", roleName),
                new SqlParameter("@RoleId", roleId)
            };
            var rows = await ExecuteNonQueryAsync(sql, ps);
            return rows > 0;
        }

        // ✅ Role delete చేయి
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            const string sql = "DELETE FROM Roles WHERE RoleId = @RoleId";
            var ps = new[] { new SqlParameter("@RoleId", roleId) };
            var rows = await ExecuteNonQueryAsync(sql, ps);
            return rows > 0;
        }

        // ✅ Role కి ఉన్న Rights తీసుకురా
        public async Task<List<Rights>> GetRightsByRoleIdAsync(int roleId)
        {
            const string sql = @"SELECT r.RightId, r.RightName, r.Description 
                                 FROM Rights r
                                 INNER JOIN RoleRights rr ON r.RightId = rr.RightId
                                 WHERE rr.RoleId = @RoleId";
            var ps = new[] { new SqlParameter("@RoleId", roleId) };
            return await ExecuteQueryAsync(sql, ps, r => new Rights
            {
                RightId = (int)r["RightId"],
                RightName = r["RightName"].ToString()!,
                Description = r["Description"] as string
            });
        }
    }
}