namespace ECommerceAPI.Repositories
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartItemDto>> GetByUserAsync(int userId);
        Task<bool> AddItemAsync(int userId, int productId, int quantity);
        Task<bool> UpdateQtyAsync(int cartItemId, int quantity);
        Task<bool> RemoveItemAsync(int cartItemId);
        Task<bool> ClearCartAsync(int userId);
    }
}
