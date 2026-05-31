namespace ECommerceAPI.Models
{
    public class RefreshToken
    {
        public int TokenId { get; set; }
        public int UserId { get; set; }

        public string TokenHash { get; set; } = "";

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
