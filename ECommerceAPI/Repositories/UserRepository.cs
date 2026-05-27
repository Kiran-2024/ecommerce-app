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
                  WHERE Email = @Email AND IsActive = 1";

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

    }
}
