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
            // 1. ముందు ఈ role కి ఉన్న RoleRights mappings తీసేయి (FK conflict avoid చేయడానికి)
            const string deleteRightsSql = "DELETE FROM RoleRights WHERE RoleId = @RoleId";
            var rightsParams = new[] { new SqlParameter("@RoleId", roleId) };
            await ExecuteNonQueryAsync(deleteRightsSql, rightsParams);

            // 2. ఇప్పుడు Role delete చేయి
            const string deleteRoleSql = "DELETE FROM Roles WHERE RoleId = @RoleId";
            var roleParams = new[] { new SqlParameter("@RoleId", roleId) };
            var rows = await ExecuteNonQueryAsync(deleteRoleSql, roleParams);
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
        // ✅ Role కి Rights assign చేయి (existing తీసేసి కొత్తవి insert చేస్తుంది - Day41 role-assign pattern లాగే)
        public async Task<bool> AssignRightsToRoleAsync(int roleId, List<int> rightIds)
        {
            // 1. ఈ role కి ఉన్న old rights తీసేయి
            const string deleteSql = "DELETE FROM RoleRights WHERE RoleId = @RoleId";
            var deleteParams = new[] { new SqlParameter("@RoleId", roleId) };
            await ExecuteNonQueryAsync(deleteSql, deleteParams);

            // 2. కొత్త rights ప్రతి ఒక్కటి insert చేయి
            const string insertSql = "INSERT INTO RoleRights (RoleId, RightId) VALUES (@RoleId, @RightId)";
            foreach (var rightId in rightIds)
            {
                var insertParams = new[]
                {
            new SqlParameter("@RoleId", roleId),
            new SqlParameter("@RightId", rightId)
        };
                await ExecuteNonQueryAsync(insertSql, insertParams);
            }
            return true;
        }
        public async Task<List<Rights>> GetAllRightsAsync()
        {
            const string sql = "SELECT RightId, RightName, Description FROM Rights ORDER BY RightId";
            return await ExecuteQueryAsync(sql, null, r => new Rights
            {
                RightId = (int)r["RightId"],
                RightName = r["RightName"].ToString()!,
                Description = r["Description"] as string
            });
        }
    }
}