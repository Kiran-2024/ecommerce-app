using ECommerceAPI.DTOs;
using ECommerceAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _repo;

        public CartController(ICartRepository repo)
        {
            _repo = repo;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var items = await _repo.GetByUserAsync(GetUserId());
            var total = items.Sum(i => i.Subtotal);
            return Ok(new { items, total });
        }

        // POST /api/cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
        {
            if (dto.Quantity <= 0)
                return BadRequest(new { message = "Quantity must be greater than 0" });

            var success = await _repo.AddItemAsync(GetUserId(), dto.ProductId, dto.Quantity);
            if (!success)
                return BadRequest(new { message = "Failed to add item" });

            return Ok(new { message = "Item added to cart" });
        }

        // PUT /api/cart/update/{cartItemId}
        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateQty(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            if (dto.Quantity <= 0)
                return BadRequest(new { message = "Quantity must be greater than 0" });

            var success = await _repo.UpdateQtyAsync(cartItemId, dto.Quantity);
            if (!success)
                return NotFound(new { message = "Cart item not found" });

            return Ok(new { message = "Quantity updated" });
        }

        // DELETE /api/cart/remove/{cartItemId}
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var success = await _repo.RemoveItemAsync(cartItemId);
            if (!success)
                return NotFound(new { message = "Cart item not found" });

            return Ok(new { message = "Item removed from cart" });
        }

        // DELETE /api/cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _repo.ClearCartAsync(GetUserId());
            return Ok(new { message = "Cart cleared" });
        }
    }
}