using ECommerceAPI.DTO_s;
using ECommerceAPI.Helpers;
using ECommerceAPI.Repositories;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepo;
        private readonly PasswordHasher _hasher;
        private readonly OtpRepository _otpRepo;
        private readonly EmailService _emailService;

        public AuthController(UserRepository userRepo, PasswordHasher hasher,
            OtpRepository otpRepo, EmailService emailService)
        {
            _userRepo = userRepo;
            _hasher = hasher;
            _otpRepo = otpRepo;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userRepo.EmailExistsAsync(dto.Email))
                return Conflict(new { message = "Email already registered." });

            if (await _userRepo.PhoneExistsAsync(dto.PhoneNumber))
                return Conflict(new { message = "Phone number already registered." });

            // Password hash చేసి User insert చేయి
            var passwordHash = _hasher.Hash(dto.Password);
            int userId = await _userRepo.InsertUserAsync(dto.FullName, dto.Email, dto.PhoneNumber, passwordHash);

            // OTP generate చేసి DB లో save చేయి
            var otpCode = OtpHelper.GenerateOtp();
            _otpRepo.InsertOtp(userId, otpCode, "EMAIL");

            // Email పంపు
            _emailService.SendOtpEmail(dto.Email, dto.FullName, otpCode);

            return Ok(new { message = "Registration successful. OTP sent to your email." });
        }

        [HttpPost("verify-email-otp")]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Email తో User తీసుకో
            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Already verified అయిందా check చేయి
            if (user.Value.IsEmailVerified)
                return BadRequest(new { message = "Email already verified." });

            // OTP validate చేయి
            bool isValid = _otpRepo.ValidateOtp(user.Value.UserId, dto.OtpCode, "EMAIL");
            if (!isValid)
                return BadRequest(new { message = "Invalid or expired OTP." });

            // OTP mark used + IsEmailVerified = true
            bool success = _otpRepo.MarkUsedAndVerifyEmail(user.Value.UserId, dto.OtpCode, "EMAIL");
            if (!success)
                return StatusCode(500, new { message = "Verification failed. Please try again." });

            return Ok(new { message = "Email verified successfully." });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Email తో User తీసుకో
            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Already verified అయిందా check చేయి
            if (user.Value.IsEmailVerified)
                return BadRequest(new { message = "Email already verified." });

            if (!_otpRepo.CanResendOtp(user.Value.UserId, "EMAIL"))
                return BadRequest(new { message = "Please wait 1 minute before requesting a new OTP." });


            // కొత్త OTP generate చేసి పంపు
            var otpCode = OtpHelper.GenerateOtp();
            _otpRepo.InsertOtp(user.Value.UserId, otpCode, "EMAIL");
            _emailService.SendOtpEmail(dto.Email, user.Value.FullName, otpCode);

            return Ok(new { message = "OTP resent successfully." });
        }

    }
}