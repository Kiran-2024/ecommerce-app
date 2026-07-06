using ECommerceAPI.Authorization;
using ECommerceApp.Models;
using ECommerceApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceApp.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderRepository _repo;

        public AdminOrdersController(IAdminOrderRepository repo)
        {
            _repo = repo;
        }

        // GET /api/admin/orders?page=1&pageSize=10&status=Pending&search=john&fromDate=&toDate=
        [HttpGet]
        [HasRight("order.manage")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var (orders, totalCount) = await _repo.GetAllOrdersAsync(
                page, pageSize, status, search, fromDate, toDate);

            return Ok(new
            {
                orders,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        // PUT /api/admin/orders/{id}/status
        [HttpPut("{id}/status")]
        [HasRight("order.manage")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid status value." });

            int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool success = await _repo.UpdateOrderStatusAsync(id, dto.Status, adminId);

            if (!success)
                return NotFound(new { message = "Order not found." });

            return Ok(new { message = $"Order status updated to {dto.Status}." });
        }
    }
}