using ECommerceAPI.DTO_s;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories;
using Microsoft.Data.SqlClient;
using System.Data;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;  

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            // 1. Cart items fetch చేయి
            var cartItems = new List<(int ProductId, string ProductName, int quantity, decimal unitPrice)>();
            decimal totalAmount = 0;

            var cartQuery = @"
                SELECT ci.ProductId, p.ProductName, ci.Quantity, p.Price
                FROM CartItems ci
                JOIN Products p ON ci.ProductId = p.ProductId
                WHERE ci.UserId = @UserId";

            using (var cartCmd = new SqlCommand(cartQuery, conn, transaction))
            {
                cartCmd.Parameters.AddWithValue("@UserId", userId);
                using var reader = await cartCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var productId = reader.GetInt32("ProductId");
                    var productName = reader.GetString("ProductName");
                    var quantity = reader.GetInt32("Quantity");
                    var unitPrice = reader.GetDecimal("Price");

                    cartItems.Add((productId, productName, quantity, unitPrice));
                    totalAmount += quantity * unitPrice;
                }
            }

            if (cartItems.Count == 0)
                throw new Exception("Cart is empty");

            // 2. Order insert చేయి
            var orderQuery = @"
                INSERT INTO Orders (UserId, AddressId, TotalAmount, OrderStatus, PaymentStatus, PaymentMethod, CreatedAt)
                VALUES (@UserId, @AddressId, @TotalAmount, 'Pending', 'Unpaid', @PaymentMethod, GETDATE());
                SELECT SCOPE_IDENTITY();";

            int orderId;
            using (var orderCmd = new SqlCommand(orderQuery, conn, transaction))
            {
                orderCmd.Parameters.AddWithValue("@UserId", userId);
                orderCmd.Parameters.AddWithValue("@AddressId", (object?)dto.AddressId ?? DBNull.Value);
                orderCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                orderCmd.Parameters.AddWithValue("@PaymentMethod", (object?)dto.PaymentMethod ?? DBNull.Value);
                orderId = Convert.ToInt32(await orderCmd.ExecuteScalarAsync());
            }

            // 3. OrderItems insert చేయి
            var orderItems = new List<OrderItemResponseDto>();
            foreach (var(productId, productName, quantity, unitPrice)  in cartItems)
            {
                var itemTotal = quantity * unitPrice;
                var itemQuery = @"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
                    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice);
                    SELECT SCOPE_IDENTITY();";

                using var itemCmd = new SqlCommand(itemQuery, conn, transaction);
                itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                itemCmd.Parameters.AddWithValue("@ProductId", productId);
                itemCmd.Parameters.AddWithValue("@Quantity", quantity);
                itemCmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                itemCmd.Parameters.AddWithValue("@TotalPrice", itemTotal);
                var orderItemId = Convert.ToInt32(await itemCmd.ExecuteScalarAsync());

                orderItems.Add(new OrderItemResponseDto
                {
                    OrderItemId = orderItemId,
                    ProductId = productId,
                    ProductName = productName,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = itemTotal
                });
            }

            // 4. Cart clear చేయి
            var clearCartQuery = @"
               DELETE FROM CartItems
               WHERE UserId = @UserId";

            using (var clearCmd = new SqlCommand(clearCartQuery, conn, transaction))
            {
                clearCmd.Parameters.AddWithValue("@UserId", userId);
                await clearCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            return new OrderResponseDto
            {
                OrderId = orderId,
                UserId = userId,
                AddressId = dto.AddressId,
                TotalAmount = totalAmount,
                OrderStatus = "Pending",
                PaymentStatus = "Unpaid",
                PaymentMethod = dto.PaymentMethod,
                CreatedAt = DateTime.Now,
                OrderItems = orderItems
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(IEnumerable<OrderResponseDto> Orders, int TotalCount)> GetOrdersByUserAsync(
          int userId, int page = 1, int pageSize = 10)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        int totalCount;

        // Total distinct orders count
        var countQuery = "SELECT COUNT(*) FROM Orders WHERE UserId = @UserId";
        using (var countCmd = new SqlCommand(countQuery, conn))
        {
            countCmd.Parameters.AddWithValue("@UserId", userId);
            totalCount = (int)await countCmd.ExecuteScalarAsync();
        }

        // Paged OrderIds first (so OFFSET-FETCH works correctly with JOIN)
        var pagedIds = new List<int>();
        int offset = (page - 1) * pageSize;

        var idsQuery = @"
            SELECT OrderId FROM Orders
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using (var idsCmd = new SqlCommand(idsQuery, conn))
        {
            idsCmd.Parameters.AddWithValue("@UserId", userId);
            idsCmd.Parameters.AddWithValue("@Offset", offset);
            idsCmd.Parameters.AddWithValue("@PageSize", pageSize);
            using var idsReader = await idsCmd.ExecuteReaderAsync();
            while (await idsReader.ReadAsync())
            {
                pagedIds.Add(idsReader.GetInt32("OrderId"));
            }
        }

        if (pagedIds.Count == 0)
            return (new List<OrderResponseDto>(), totalCount);

        // Fetch those orders with items via JOIN
        var orders = new Dictionary<int, OrderResponseDto>();
        var idsParam = string.Join(",", pagedIds);

        var query = $@"
            SELECT o.OrderId, o.UserId, o.AddressId, o.TotalAmount, o.OrderStatus,
                   o.PaymentStatus, o.PaymentMethod, o.CreatedAt,
                   oi.OrderItemId, oi.ProductId, p.ProductName, oi.Quantity, oi.UnitPrice, oi.TotalPrice
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
            LEFT JOIN Products p ON oi.ProductId = p.ProductId
            WHERE o.OrderId IN ({idsParam})
            ORDER BY o.CreatedAt DESC";

        using var cmd = new SqlCommand(query, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var orderId = reader.GetInt32("OrderId");
            if (!orders.ContainsKey(orderId))
            {
                orders[orderId] = new OrderResponseDto
                {
                    OrderId = orderId,
                    UserId = reader.GetInt32("UserId"),
                    AddressId = reader.IsDBNull("AddressId") ? null : reader.GetInt32("AddressId"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    OrderStatus = reader.GetString("OrderStatus"),
                    PaymentStatus = reader.IsDBNull("PaymentStatus") ? null : reader.GetString("PaymentStatus"),
                    PaymentMethod = reader.IsDBNull("PaymentMethod") ? null : reader.GetString("PaymentMethod"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    OrderItems = new List<OrderItemResponseDto>()
                };
            }

            if (!reader.IsDBNull("OrderItemId"))
            {
                orders[orderId].OrderItems.Add(new OrderItemResponseDto
                {
                    OrderItemId = reader.GetInt32("OrderItemId"),
                    ProductId = reader.GetInt32("ProductId"),
                    ProductName = reader.GetString("ProductName"),
                    Quantity = reader.GetInt32("Quantity"),
                    UnitPrice = reader.GetDecimal("UnitPrice"),
                    TotalPrice = reader.GetDecimal("TotalPrice")
                });
            }
        }

        // Preserve page order (Dictionary doesn't guarantee order)
        var orderedResult = pagedIds.Select(id => orders[id]).ToList();

        return (orderedResult, totalCount);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int orderId, int userId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        OrderResponseDto? order = null;

        var query = @"
            SELECT o.OrderId, o.UserId, o.AddressId, o.TotalAmount, o.OrderStatus,
                   o.PaymentStatus, o.PaymentMethod, o.CreatedAt,
                   oi.OrderItemId, oi.ProductId, p.ProductName, oi.Quantity, oi.UnitPrice, oi.TotalPrice
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
            LEFT JOIN Products p ON oi.ProductId = p.ProductId
            WHERE o.OrderId = @OrderId AND o.UserId = @UserId";

        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@OrderId", orderId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (order == null)
            {
                order = new OrderResponseDto
                {
                    OrderId = reader.GetInt32("OrderId"),
                    UserId = reader.GetInt32("UserId"),
                    AddressId = reader.IsDBNull("AddressId") ? null : reader.GetInt32("AddressId"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    OrderStatus = reader.GetString("OrderStatus"),
                    PaymentStatus = reader.IsDBNull("PaymentStatus") ? null : reader.GetString("PaymentStatus"),
                    PaymentMethod = reader.IsDBNull("PaymentMethod") ? null : reader.GetString("PaymentMethod"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    OrderItems = new List<OrderItemResponseDto>()
                };
            }

            if (!reader.IsDBNull("OrderItemId"))
            {
                order.OrderItems.Add(new OrderItemResponseDto
                {
                    OrderItemId = reader.GetInt32("OrderItemId"),
                    ProductId = reader.GetInt32("ProductId"),
                    ProductName = reader.GetString("ProductName"),
                    Quantity = reader.GetInt32("Quantity"),
                    UnitPrice = reader.GetDecimal("UnitPrice"),
                    TotalPrice = reader.GetDecimal("TotalPrice")
                });
            }
        }

        return order;
    }

    public async Task<bool> CancelOrderAsync(int orderId, int userId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var query = @"
            UPDATE Orders 
            SET OrderStatus = 'Cancelled'
            WHERE OrderId = @OrderId AND UserId = @UserId AND OrderStatus = 'Pending'";

        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@OrderId", orderId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }
}