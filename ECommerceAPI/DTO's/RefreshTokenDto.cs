using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTO_s
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = "";

    }

    public class LogoutDto
    {
        [Required]
        public string RefreshToken { get; set; } = "";
    }
}
