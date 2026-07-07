using ECommerceAPI.Data;
using ECommerceAPI.Repositories.Base;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories
{
    public class UserRepository:BaseRepository
    {
        public UserRepository(DatabaseHelper db) : base(db) { }

        public  async Task<bool> EmailExistsAsync(string email)
        {
            var query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            var parameters = new[] { new SqlParameter("@Email", email) };
            var result = await ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            var query = "SELECT COUNT(1) FROM Users WHERE PhoneNumber = @Phone";
            var parameters = new[] { new SqlParameter("@Phone", phone) };
            var result = await ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }
        public async Task<int> InsertUserAsync(string fullName, string email, string phone, string passwordHash)
        {
            var query = @"INSERT INTO Users 
                  (FullName, Email, PhoneNumber, PasswordHash, CreatedAt, IsEmailVerified, IsPhoneVerified, IsActive)
                  VALUES 
                  (@FullName, @Email, @Phone, @PasswordHash, GETDATE(), 0, 0, 1);
                  SELECT SCOPE_IDENTITY();";

            var parameters = new[]
            {
        new SqlParameter("@FullName", fullName),
        new SqlParameter("@Email", email),
        new SqlParameter("@Phone", phone),
        new SqlParameter("@PasswordHash", passwordHash),
    };

            var result = await ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }
        public async Task<(int UserId, string FullName, bool IsEmailVerified)?> GetUserByEmailAsync(string email)
        {
            var query = @"SELECT UserId, FullName, IsEmailVerified 
                  FROM Users 
                  WHERE Email = @Email";

            var parameters = new[] { new SqlParameter("@Email", email) };

            var result = await ExecuteQueryAsync(query, parameters, reader => (
                UserId: reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName: reader.GetString(reader.GetOrdinal("FullName")),
                IsEmailVerified: reader.GetBoolean(reader.GetOrdinal("IsEmailVerified"))
            ));

            return result.FirstOrDefault();
        }

        public async Task UpdateEmailVerifiedAsync(int userId)
        {
            var query = @"UPDATE Users 
                  SET IsEmailVerified = 1 
                  WHERE UserId = @UserId";

            var parameters = new[] { new SqlParameter("@UserId", userId) };

            await ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<(int UserId, string FullName, string PasswordHash, string Role, bool IsEmailVerified)?> GetUserForLoginAsync(string email)
        {
            var query = @"
        SELECT u.UserId, u.FullName, u.PasswordHash, u.IsEmailVerified,
               r.RoleName
        FROM Users u
        LEFT JOIN UserRoles ur ON ur.UserId = u.UserId
        LEFT JOIN Roles r ON r.RoleId = ur.RoleId
        WHERE u.Email = @Email";

            var parameters = new[] { new SqlParameter("@Email", email) };

            var result = await ExecuteQueryAsync(query, parameters, reader => (
                UserId: reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName: reader.GetString(reader.GetOrdinal("FullName")),
                PasswordHash: reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role: reader.IsDBNull(reader.GetOrdinal("RoleName"))
                             ? "Customer"
                             : reader.GetString(reader.GetOrdinal("RoleName")),
                IsEmailVerified: reader.GetBoolean(reader.GetOrdinal("IsEmailVerified"))
            ));

            return result.FirstOrDefault();
        }
        public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            string query = "UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = @UpdatedAt WHERE UserId = @UserId";
            var parameters = new[]
            {
        new SqlParameter("@PasswordHash", newPasswordHash),
        new SqlParameter("@UpdatedAt", DateTime.UtcNow),
        new SqlParameter("@UserId", userId)
    };
            int rows = await ExecuteNonQueryAsync(query, parameters);
            return rows > 0;
        }
        public async Task<(int UserId, string FullName, string Role)?> GetUserByIdAsync(int userId)
        {
            var query = @"
        SELECT u.UserId, u.FullName, r.RoleName
        FROM Users u
        LEFT JOIN UserRoles ur ON ur.UserId = u.UserId
        LEFT JOIN Roles r ON r.RoleId = ur.RoleId
        WHERE u.UserId = @UserId";

            var parameters = new[] { new SqlParameter("@UserId", userId) };

            var result = await ExecuteQueryAsync(query, parameters, reader => (
                UserId: reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName: reader.GetString(reader.GetOrdinal("FullName")),
                Role: reader.IsDBNull(reader.GetOrdinal("RoleName"))
                             ? "Customer"
                             : reader.GetString(reader.GetOrdinal("RoleName"))
            ));

            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<string>> GetUserRightsAsync(int userId)
        {
            var query = @"
        SELECT rg.RightName
        FROM Users u
        LEFT JOIN UserRoles ur ON ur.UserId = u.UserId
        LEFT JOIN RoleRights rr ON rr.RoleId = ur.RoleId
        LEFT JOIN Rights rg ON rg.RightId = rr.RightId
        WHERE u.UserId = @UserId";

            var parameters = new[] { new SqlParameter("@UserId", userId) };

            var result = await ExecuteQueryAsync(query, parameters, reader =>
                reader.IsDBNull(reader.GetOrdinal("RightName"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("RightName")));

            return result.Where(r => r != null).Select(r => r!);
        }


        public async Task<(IEnumerable<(int UserId, string FullName, string Email, string PhoneNumber,
            string Role, bool IsActive, bool IsEmailVerified, DateTime CreatedAt)> Data, int TotalCount)>
            GetAllUsersAsync(int page, int pageSize, string? search, string? role)
        {
            var whereClauses = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClauses.Add("(u.FullName LIKE @Search OR u.Email LIKE @Search OR u.PhoneNumber LIKE @Search)");
                parameters.Add(new SqlParameter("@Search", $"%{search}%"));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                whereClauses.Add("r.RoleName = @Role");
                parameters.Add(new SqlParameter("@Role", role));
            }

            string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            // total count
            var countQuery = $@"
                SELECT COUNT(1)
                FROM Users u
                LEFT JOIN UserRoles ur ON ur.UserId = u.UserId
                LEFT JOIN Roles r ON r.RoleId = ur.RoleId
                {whereSql}";

            var countParams = parameters.Select(p => new SqlParameter(p.ParameterName, p.Value)).ToArray();
            var totalCount = Convert.ToInt32(await ExecuteScalarAsync(countQuery, countParams));

            // paged data
            var dataQuery = $@"
                SELECT u.UserId, u.FullName, u.Email, u.PhoneNumber,
                       ISNULL(r.RoleName, 'Customer') AS Role,
                       u.IsActive, u.IsEmailVerified, u.CreatedAt
                FROM Users u
                LEFT JOIN UserRoles ur ON ur.UserId = u.UserId
                LEFT JOIN Roles r ON r.RoleId = ur.RoleId
                {whereSql}
                ORDER BY u.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var dataParams = parameters.Select(p => new SqlParameter(p.ParameterName, p.Value)).ToList();
            dataParams.Add(new SqlParameter("@Offset", (page - 1) * pageSize));
            dataParams.Add(new SqlParameter("@PageSize", pageSize));

            var data = await ExecuteQueryAsync(dataQuery, dataParams.ToArray(), reader => (
                UserId: reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName: reader.GetString(reader.GetOrdinal("FullName")),
                Email: reader.GetString(reader.GetOrdinal("Email")),
                PhoneNumber: reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "" : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                Role: reader.GetString(reader.GetOrdinal("Role")),
                IsActive: reader.GetBoolean(reader.GetOrdinal("IsActive")),
                IsEmailVerified: reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                CreatedAt: reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            ));

            return (data, totalCount);
        }
        public async Task<bool> UpdateUserActiveStatusAsync(int userId, bool isActive)
        {
            var query = "UPDATE Users SET IsActive = @IsActive WHERE UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UserId", userId)
            };
            int rows = await ExecuteNonQueryAsync(query, parameters);
            return rows > 0;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, int roleId)
        {
            // remove existing role mapping, assign new one (single-role-per-user model)
            var deleteQuery = "DELETE FROM UserRoles WHERE UserId = @UserId";
            await ExecuteNonQueryAsync(deleteQuery, new[] { new SqlParameter("@UserId", userId) });

            var insertQuery = "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@RoleId", roleId)
            };
            int rows = await ExecuteNonQueryAsync(insertQuery, parameters);
            return rows > 0;
        }

        public async Task<IEnumerable<(int RoleId, string RoleName)>> GetAllRolesAsync()
        {
            var query = "SELECT RoleId, RoleName FROM Roles ORDER BY RoleName";
            var result = await ExecuteQueryAsync(query, Array.Empty<SqlParameter>(), reader => (
                RoleId: reader.GetInt32(reader.GetOrdinal("RoleId")),
                RoleName: reader.GetString(reader.GetOrdinal("RoleName"))
            ));
            return result;
        }

    }
}
