using ECommerceAPI.Data;
using ECommerceAPI.Repositories.Base;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories
{
    public class RoleRightsRepository : BaseRepository
    {
        public RoleRightsRepository(DatabaseHelper db) : base(db) { }

        public async Task<IEnumerable<string>> GetRightsByUserIdAsync(int userId)
        {
            var query = @"
                SELECT DISTINCT rg.RightName
                FROM Rights rg
                INNER JOIN RoleRights rr ON rr.RightId = rg.RightId
                INNER JOIN UserRoles  ur ON ur.RoleId  = rr.RoleId
                WHERE ur.UserId = @UserId";

            var parameters = new[] { new SqlParameter("@UserId", userId) };

            return await ExecuteQueryAsync(query, parameters, reader =>
                reader.GetString(reader.GetOrdinal("RightName"))
            );
        }

        public async Task AssignRoleToUserAsync(int userId, int roleId)
        {
            var query = @"
                IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
                    INSERT INTO UserRoles (UserId, RoleId, AssignedAt) 
                    VALUES (@UserId, @RoleId, GETDATE())";

            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@RoleId", roleId)
            };

            await ExecuteNonQueryAsync(query, parameters);
        }
    }
}