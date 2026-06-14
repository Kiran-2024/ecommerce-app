using ECommerceAPI.DTO_s;

namespace ECommerceAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderDto dto);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserAsync(int userId);
        Task<OrderResponseDto?> GetOrderByIdAsync(int orderId, int userId);
        Task<bool> CancelOrderAsync(int orderId, int userId);
    }
}
