using ECommerceAPI.DTOs;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECommerceAPI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly string _connStr;

        public CartRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection")!;
        }

        // GET - User cart items
        public async Task<IEnumerable<CartItemDto>> GetByUserAsync(int userId)
        {
            var items = new List<CartItemDto>();
            using var conn = new SqlConnection(_connStr);
            var cmd = new SqlCommand(@"
                SELECT ci.CartItemId, ci.ProductId, p.ProductName, 
                       p.ImageUrl, p.Price, ci.Quantity, ci.AddedAt
                FROM CartItems ci
                INNER JOIN Products p ON ci.ProductId = p.ProductId
                WHERE ci.UserId = @UserId AND p.IsDeleted = 0", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new CartItemDto
                {
                    CartItemId = reader.GetInt32("CartItemId"),
                    ProductId = reader.GetInt32("ProductId"),
                    ProductName = reader.GetString("ProductName"),
                    ImageUrl = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl"),
                    Price = reader.GetDecimal("Price"),
                    Quantity = reader.GetInt32("Quantity"),
                    AddedAt = reader.GetDateTime("AddedAt")
                });
            }
            return items;
        }

        // ADD - Cart lo item add
        public async Task<bool> AddItemAsync(int userId, int productId, int quantity)
        {
            using var conn = new SqlConnection(_connStr);
            var cmd = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM CartItems WHERE UserId = @UserId AND ProductId = @ProductId)
                    UPDATE CartItems 
                    SET Quantity = Quantity + @Quantity
                    WHERE UserId = @UserId AND ProductId = @ProductId
                ELSE
                    INSERT INTO CartItems (UserId, ProductId, Quantity)
                    VALUES (@UserId, @ProductId, @Quantity)", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // UPDATE - Quantity update
        public async Task<bool> UpdateQtyAsync(int cartItemId, int quantity)
        {
            using var conn = new SqlConnection(_connStr);
            var cmd = new SqlCommand(@"
                UPDATE CartItems SET Quantity = @Quantity 
                WHERE CartItemId = @CartItemId", conn);

            cmd.Parameters.AddWithValue("@CartItemId", cartItemId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // REMOVE - Single item remove
        public async Task<bool> RemoveItemAsync(int cartItemId)
        {
            using var conn = new SqlConnection(_connStr);
            var cmd = new SqlCommand(
                "DELETE FROM CartItems WHERE CartItemId = @CartItemId", conn);

            cmd.Parameters.AddWithValue("@CartItemId", cartItemId);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // CLEAR - User cart clear
        public async Task<bool> ClearCartAsync(int userId)
        {
            using var conn = new SqlConnection(_connStr);
            var cmd = new SqlCommand(
                "DELETE FROM CartItems WHERE UserId = @UserId", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}