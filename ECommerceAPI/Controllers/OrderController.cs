using ECommerceAPI.DTO_s;
using ECommerceAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;

    public OrderController(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // POST: api/order/checkout
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CreateOrderDto dto)
    {
        try
        {
            var userId = GetUserId();
            var order = await _orderRepository.CreateOrderAsync(userId, dto);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/order/myorders
    [HttpGet("myorders")]
    public async Task<IActionResult> GetMyOrders(int page = 1, int pageSize = 10)
    {
        var userId = GetUserId();
        var (orders, totalCount) = await _orderRepository.GetOrdersByUserAsync(userId, page, pageSize);
        return Ok(new
        {
            data = orders,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    // GET: api/order/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var userId = GetUserId();
        var order = await _orderRepository.GetOrderByIdAsync(id, userId);
        if (order == null)
            return NotFound(new { message = "Order not found" });
        return Ok(order);
    }

    // PUT: api/order/{id}/cancel
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = GetUserId();
        var result = await _orderRepository.CancelOrderAsync(id, userId);
        if (!result)
            return BadRequest(new { message = "Order cannot be cancelled" });
        return Ok(new { message = "Order cancelled successfully" });
    }
}