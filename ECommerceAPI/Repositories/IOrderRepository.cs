using ECommerceAPI.DTO_s;
using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderDto dto);
        Task<(IEnumerable<OrderResponseDto> Orders, int TotalCount)> GetOrdersByUserAsync(
           int userId, int page = 1, int pageSize = 10);
        Task<OrderResponseDto?> GetOrderByIdAsync(int orderId, int userId);
        Task<bool> CancelOrderAsync(int orderId, int userId);

        Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId);
        Task AddOrderStatusHistoryAsync(int orderId, string status, string changedBy);
    }
}
