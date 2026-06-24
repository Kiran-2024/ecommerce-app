using ECommerceAPI.DTO_s;
using ECommerceAPI.Repositories;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly EmailService _emailService;
    private readonly InvoiceService _invoiceService;

    public OrderController(IOrderRepository orderRepository, EmailService emailService, InvoiceService invoiceService)
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
        _invoiceService = invoiceService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email)!;

    // POST: api/order/checkout
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CreateOrderDto dto)
    {
        try
        {
            var userId = GetUserId();
            var order = await _orderRepository.CreateOrderAsync(userId, dto);

            // Order confirmation email — fail aithe order ki harm cheyakudadu
            try
            {
                var email = GetUserEmail();
                var items = order.OrderItems
                    .Select(i => (i.ProductName, i.Quantity, i.TotalPrice))
                    .ToList();

                _emailService.SendOrderConfirmation(
                    email, "there", order.OrderId, order.TotalAmount, order.PaymentMethod ?? "N/A", items);
            }
            catch (Exception emailEx)
            {
                Console.WriteLine($"Order confirmation email failed: {emailEx.Message}");
            }

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

        // Status update email — fail aithe response ki harm cheyakudadu
        try
        {
            var email = GetUserEmail();
            _emailService.SendStatusUpdate(email, "there", id, "Cancelled");
        }
        catch (Exception emailEx)
        {
            Console.WriteLine($"Status update email failed: {emailEx.Message}");
        }

        return Ok(new { message = "Order cancelled successfully" });
    }

    [HttpGet("{id}/invoice")]
    [Authorize]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var order = await _orderRepository.GetOrderByIdAsync(id, userId);
        if (order == null)
            return NotFound("Order not found.");

        var pdfBytes = _invoiceService.GenerateInvoice(order);

        return File(pdfBytes, "application/pdf", $"Invoice_Order_{id}.pdf");
    }
}