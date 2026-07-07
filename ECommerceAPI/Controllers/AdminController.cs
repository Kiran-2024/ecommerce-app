using ECommerceAPI.Authorization;
using ECommerceAPI.Models.DTOs;
using ECommerceAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;
        private readonly UserRepository _userRepository;

        public AdminController(IAdminRepository adminRepository, UserRepository userRepository)
        {
            _adminRepository = adminRepository;
            _userRepository = userRepository;
        }

        [HttpGet("dashboard")]
        [HasRight("dashboard.view")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminRepository.GetDashboardStatsAsync();
            return Ok(dashboard);
        }

        // ============ Day 41: Admin User Management ============

        [HttpGet("users")]
        [HasRight("user.manage")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            var (data, totalCount) = await _userRepository.GetAllUsersAsync(page, pageSize, search, role);

            var mapped = data.Select(u => new AdminUserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(new
            {
                data = mapped,
                totalCount,
                page,
                pageSize
            });
        }

        [HttpPut("users/{id}/activate-deactivate")]
        [HasRight("user.manage")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            var result = await _userRepository.UpdateUserActiveStatusAsync(id, dto.IsActive);
            if (!result)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = dto.IsActive ? "User activated" : "User deactivated" });
        }

        [HttpPut("users/{id}/role")]
        [HasRight("user.manage")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            var result = await _userRepository.UpdateUserRoleAsync(id, dto.RoleId);
            if (!result)
                return BadRequest(new { message = "Failed to update role" });

            return Ok(new { message = "Role updated successfully" });
        }

        [HttpGet("roles")]
        [HasRight("user.manage")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _userRepository.GetAllRolesAsync();
            var mapped = roles.Select(r => new RoleDto { RoleId = r.RoleId, RoleName = r.RoleName });
            return Ok(mapped);
        }
    }
}