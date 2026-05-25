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
    }
}