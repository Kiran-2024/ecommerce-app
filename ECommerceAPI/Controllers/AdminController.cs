using ECommerceAPI.Authorization;
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

        public AdminController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        [HttpGet("dashboard")]
        [HasRight("dashboard.view")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminRepository.GetDashboardStatsAsync();
            return Ok(dashboard);
        }
    }
}