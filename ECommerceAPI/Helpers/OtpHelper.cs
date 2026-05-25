namespace ECommerceAPI.Helpers
{
    public static  class OtpHelper
    {
        public static string GenerateOtp()
        {
            Random random = new Random();
            int otp = random.Next(100000, 999999);
            return otp.ToString();
        }
    }
}
