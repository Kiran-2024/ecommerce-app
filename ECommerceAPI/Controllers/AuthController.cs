using ECommerceAPI.DTO_s;
using ECommerceAPI.Helpers;
using ECommerceAPI.Repositories;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

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
        private readonly JwtHelper _jwtHelper;
        private readonly RoleRightsRepository _roleRightsRepository;

        public AuthController(UserRepository userRepo, PasswordHasher hasher,
            OtpRepository otpRepo, EmailService emailService,JwtHelper jwtHelper, RoleRightsRepository roleRightsRepository)
        {
            _userRepo = userRepo;
            _hasher = hasher;
            _otpRepo = otpRepo;
            _emailService = emailService;
            _jwtHelper = jwtHelper;
            _roleRightsRepository = roleRightsRepository;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // User తీసుకో
            var user = await _userRepo.GetUserForLoginAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            // Email verified అయిందా check చేయి
            if (!user.Value.IsEmailVerified)
                return Unauthorized(new { message = "Please verify your email before logging in." });

            // Password validate చేయి
            bool isValid = _hasher.Verify(dto.Password, user.Value.PasswordHash);
            if (!isValid)
                return Unauthorized(new { message = "Invalid email or password." });

            // JWT generate చేయి
            var rights = await _roleRightsRepository.GetRightsByUserIdAsync(user.Value.UserId);

            // Token generate చేయండి
            string token = _jwtHelper.GenerateToken(
                user.Value.UserId,
                dto.Email,
                user.Value.Role,
                rights
            );

            return Ok(new LoginResponseDto
            {
                Token = token,
                Email = dto.Email,
                FullName = user.Value.FullName,
                Role = user.Value.Role
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!user.Value.IsEmailVerified)
                return BadRequest(new { message = "Please verify your email first." });

            var otpCode = OtpHelper.GenerateOtp();
            _otpRepo.InsertOtp(user.Value.UserId, otpCode, "RESET");
            _emailService.SendOtpEmail(dto.Email, user.Value.FullName, otpCode);

            return Ok(new { message = "Password reset OTP sent to your email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            bool isValid = _otpRepo.ValidateOtp(user.Value.UserId, dto.OtpCode, "RESET");
            if (!isValid)
                return BadRequest(new { message = "Invalid or expired OTP." });

            _otpRepo.MarkUsedAndVerifyEmail(user.Value.UserId, dto.OtpCode, "RESET");

            var newHash = _hasher.Hash(dto.NewPassword);
            bool success = await _userRepo.UpdatePasswordAsync(user.Value.UserId, newHash);
            if (!success)
                return StatusCode(500, new { message = "Password update failed." });

            return Ok(new { message = "Password reset successfully." });
        }
    }
}