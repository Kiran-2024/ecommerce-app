using ECommerceAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRolesRepository _rolesRepo;

        public RolesController(IRolesRepository rolesRepo)
        {
            _rolesRepo = rolesRepo;
        }

        // GET api/roles
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _rolesRepo.GetAllRolesAsync();
            return Ok(roles);
        }

        // GET api/roles/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _rolesRepo.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound("Role not found");
            return Ok(role);
        }

        // GET api/roles/1/rights
        [HttpGet("{id}/rights")]
        public async Task<IActionResult> GetRightsByRole(int id)
        {
            var rights = await _rolesRepo.GetRightsByRoleIdAsync(id);
            return Ok(rights);
        }
        // POST api/roles
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Role name required");
            var newId = await _rolesRepo.CreateRoleAsync(roleName);
            return Ok(new { RoleId = newId, RoleName = roleName });
        }

        // PUT api/roles/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] string roleName)
        {
            var updated = await _rolesRepo.UpdateRoleAsync(id, roleName);
            if (!updated)
                return NotFound("Role not found");
            return Ok("Role updated successfully");
        }

        // DELETE api/roles/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var deleted = await _rolesRepo.DeleteRoleAsync(id);
            if (!deleted)
                return NotFound("Role not found");
            return Ok("Role deleted successfully");
        }
    }
}
