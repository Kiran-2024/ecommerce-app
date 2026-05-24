using ECommerceAPI.DTO_s;
using ECommerceAPI.Helpers;
using ECommerceAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepo;
        private readonly PasswordHasher _hasher;
        public AuthController(UserRepository userRepo,PasswordHasher hasher)
        {
            _userRepo = userRepo;
            _hasher = hasher;
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

            var passwordHash = _hasher.Hash(dto.Password);
            await _userRepo.InsertUserAsync(dto.FullName, dto.Email, dto.PhoneNumber, passwordHash);

            return Ok(new { message = "Registration successful." });
        }
    }
}
