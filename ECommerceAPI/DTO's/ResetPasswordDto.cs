using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTO_s
{
    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OtpCode { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
