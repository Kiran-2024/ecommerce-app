using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Repositories
{
    public class OtpRepository
    {
        private readonly string _connectionString;

        public OtpRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void InsertOtp(int userId, string otpCode, string otpType)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(@"
                INSERT INTO OtpVerifications (UserId, OtpCode, OtpType, IsUsed, ExpiresAt, CreatedAt)
                VALUES (@UserId, @OtpCode, @OtpType, 0, @ExpiresAt, GETDATE())", con);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@OtpCode", otpCode);
            cmd.Parameters.AddWithValue("@OtpType", otpType);
            cmd.Parameters.AddWithValue("@ExpiresAt", DateTime.Now.AddMinutes(15));

            con.Open();
            cmd.ExecuteNonQuery();
        }


        public bool ValidateOtp(int userId, string otpCode, string otpType)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM OtpVerifications
                WHERE UserId = @UserId
                  AND OtpCode = @OtpCode
                  AND OtpType = @OtpType
                  AND IsUsed = 0
                  AND ExpiresAt > GETDATE()", con);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@OtpCode", otpCode);
            cmd.Parameters.AddWithValue("@OtpType", otpType);

            con.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        public void MarkUsed(int userId, string otpCode, string otpType)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(@"
                UPDATE OtpVerifications
                SET IsUsed = 1
                WHERE UserId = @UserId
                  AND OtpCode = @OtpCode
                  AND OtpType = @OtpType
                  AND IsUsed = 0", con);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@OtpCode", otpCode);
            cmd.Parameters.AddWithValue("@OtpType", otpType);

            con.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
