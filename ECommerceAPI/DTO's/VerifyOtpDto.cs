using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTO_s
{
    public class VerifyOtpDto
    {
        [Required]
        [EmailAddress]

        public string Email { get; set; }

        [Required]
        [StringLength(6,MinimumLength = 6)]
        public string OtpCode { get; set; }
    }
    public class ResendOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
