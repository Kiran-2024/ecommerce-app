using ECommerceAPI.DTO_s;
using ECommerceAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repo;

        public CategoriesController(ICategoryRepository repo)
        {
            _repo = repo;
        }

        // GET /api/categories  (Public)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _repo.GetAllAsync();
            return Ok(categories);
        }

        // GET /api/categories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return NotFound("Category not found");
            return Ok(category);
        }

        // POST /api/categories  (Admin only)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var newId = await _repo.InsertAsync(dto);
            return Ok(new { message = "Category created", categoryId = newId });
        }

        // PUT /api/categories/5  (Admin only)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _repo.UpdateAsync(id, dto);
            if (!success) return NotFound("Category not found");
            return Ok(new { message = "Category updated" });
        }

        // DELETE /api/categories/5  (Admin only - soft delete)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _repo.DeleteAsync(id);
            if (!success) return NotFound("Category not found");
            return Ok(new { message = "Category deleted" });
        }
        [HttpGet("admin/all")]
        [Authorize]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var categories = await _repo.GetAllForAdminAsync();
            return Ok(categories);
        }
    }
}