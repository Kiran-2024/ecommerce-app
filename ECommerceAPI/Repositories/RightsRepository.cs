using ECommerceAPI.Data;
using ECommerceAPI.Repositories.Base;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories
{
    public class RightsRepository : BaseRepository
    {
        public RightsRepository(DatabaseHelper db) : base(db) { }

        public async Task<IEnumerable<string>> GetRightsByRoleIdAsync(int roleId)
        {
            var query = @"
                SELECT r.RightName 
                FROM Rights r
                INNER JOIN RoleRights rr ON rr.RightId = r.RightId
                WHERE rr.RoleId = @RoleId";

            var parameters = new[] { new SqlParameter("@RoleId", roleId) };

            return await ExecuteQueryAsync(query, parameters, reader =>
                reader.GetString(reader.GetOrdinal("RightName"))
            );
        }
    }
}