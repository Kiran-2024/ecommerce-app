using Microsoft.Data.SqlClient;
using ECommerceAPI.Data;
using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories
{
    public class RefreshTokenRepository
    {
        private readonly DatabaseHelper _db;

        public RefreshTokenRepository(DatabaseHelper db) => _db = db;

        public void SaveToken(int userId, string tokenHash, DateTime expiresAt)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            string sql = @"INSERT INTO RefreshTokens (UserId, TokenHash, ExpiresAt)
                           VALUES (@UserId, @TokenHash, @ExpiresAt)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@TokenHash", tokenHash);
            cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
            cmd.ExecuteNonQuery();
        }

        public RefreshToken? GetByTokenHash(string tokenHash)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            string sql = @"SELECT TokenId, UserId, TokenHash, ExpiresAt 
                   FROM RefreshTokens
                   WHERE TokenHash = @TokenHash 
                   AND IsRevoked = 0 
                   AND ExpiresAt > GETDATE()";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TokenHash", tokenHash);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new RefreshToken
            {
                TokenId = reader.GetInt32(reader.GetOrdinal("TokenId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                TokenHash = reader.GetString(reader.GetOrdinal("TokenHash")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt"))
            };
        }

        public void RevokeToken(string tokenHash)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            string sql = @"UPDATE RefreshTokens 
                           SET IsRevoked = 1, RevokedAt = GETDATE()
                           WHERE TokenHash = @TokenHash";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TokenHash", tokenHash);
            cmd.ExecuteNonQuery();
        }

        public void RevokeAllUserTokens(int userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            string sql = @"UPDATE RefreshTokens 
                           SET IsRevoked = 1, RevokedAt = GETDATE()
                           WHERE UserId = @UserId AND IsRevoked = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}