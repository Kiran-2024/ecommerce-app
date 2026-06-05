using ECommerceAPI.Authorization;
using ECommerceAPI.DTOs;
using ECommerceAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repo;

        public ProductsController(IProductRepository repo)
        {
            _repo = repo;
        }

        // GET /api/products?page=1&pageSize=10
       
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts(
    int page = 1, int pageSize = 10,
    string? search = null, int? categoryId = null,
    decimal? minPrice = null, decimal? maxPrice = null)
        {
            var (products, totalCount) = await _repo.GetAllAsync(
                page, pageSize, search, categoryId, minPrice, maxPrice);
            return Ok(new
            {
                data = products,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        // GET /api/products/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return NotFound("Product not found");
            return Ok(product);
        }

        // POST /api/products
        [HttpPost]
        [HasRight("product.create")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var newId = await _repo.InsertAsync(dto);
            return Ok(new { message = "Product created", productId = newId });
        }

        // PUT /api/products/5
        [HttpPut("{id}")]
        [HasRight("product.edit")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _repo.UpdateAsync(id, dto);
            if (!success) return NotFound("Product not found");
            return Ok(new { message = "Product updated" });
        }

        // DELETE /api/products/5
        [HttpDelete("{id}")]
        [HasRight("product.delete")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _repo.SoftDeleteAsync(id);
            if (!success) return NotFound("Product not found");
            return Ok(new { message = "Product deleted" });
        }
    }
}