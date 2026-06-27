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
            // 1. Cart items fetch చేయి (Stock కూడా)
            var cartItems = new List<(int ProductId, string ProductName, int Quantity, decimal UnitPrice, int Stock)>();
            decimal totalAmount = 0;

            var cartQuery = @"
                SELECT ci.ProductId, p.ProductName, ci.Quantity, p.Price, p.Stock
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
                    var stock = reader.GetInt32("Stock");

                    cartItems.Add((productId, productName, quantity, unitPrice, stock));
                    totalAmount += quantity * unitPrice;
                }
            }

            if (cartItems.Count == 0)
                throw new Exception("Cart is empty");

            // 1.5. Stock validation — em product ki stock chaladu ante block cheyali
            var outOfStockItems = cartItems.Where(c => c.Quantity > c.Stock).ToList();
            if (outOfStockItems.Any())
            {
                var names = string.Join(", ", outOfStockItems.Select(c => c.ProductName));
                throw new Exception($"Insufficient stock for: {names}");
            }

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

            // 3. OrderItems insert చేయి + Stock deduct చేయి
            var orderItems = new List<OrderItemResponseDto>();
            foreach (var (productId, productName, quantity, unitPrice, stock) in cartItems)
            {
                var itemTotal = quantity * unitPrice;
                var itemQuery = @"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
                    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice);
                    SELECT SCOPE_IDENTITY();";

                using (var itemCmd = new SqlCommand(itemQuery, conn, transaction))
                {
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

                // Stock deduct చేయి
                var stockUpdateQuery = @"
                    UPDATE Products SET Stock = Stock - @Quantity
                    WHERE ProductId = @ProductId";

                using (var stockCmd = new SqlCommand(stockUpdateQuery, conn, transaction))
                {
                    stockCmd.Parameters.AddWithValue("@Quantity", quantity);
                    stockCmd.Parameters.AddWithValue("@ProductId", productId);
                    await stockCmd.ExecuteNonQueryAsync();
                }
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
        using var transaction = conn.BeginTransaction();

        try
        {
            // 1. Order ni Pending state lo unte matrame Cancelled ki update cheyali
            var cancelQuery = @"
                UPDATE Orders 
                SET OrderStatus = 'Cancelled'
                WHERE OrderId = @OrderId AND UserId = @UserId AND OrderStatus = 'Pending'";

            int rows;
            using (var cancelCmd = new SqlCommand(cancelQuery, conn, transaction))
            {
                cancelCmd.Parameters.AddWithValue("@OrderId", orderId);
                cancelCmd.Parameters.AddWithValue("@UserId", userId);
                rows = await cancelCmd.ExecuteNonQueryAsync();
            }

            // Order Pending lo lekapote (already Shipped/Delivered/Cancelled) - cancel cheyaledu
            if (rows == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // 2. Ee order item products ki Stock restore cheyali
            var restoreStockQuery = @"
                UPDATE p
                SET p.Stock = p.Stock + oi.Quantity
                FROM Products p
                JOIN OrderItems oi ON p.ProductId = oi.ProductId
                WHERE oi.OrderId = @OrderId";

            using (var restoreCmd = new SqlCommand(restoreStockQuery, conn, transaction))
            {
                restoreCmd.Parameters.AddWithValue("@OrderId", orderId);
                await restoreCmd.ExecuteNonQueryAsync();
            }
            var historyQuery = @"
             INSERT INTO OrderStatusHistory (OrderId, Status, ChangedBy)
             VALUES (@OrderId, @Status, @ChangedBy)";

            using (var historyCmd = new SqlCommand(historyQuery, conn, transaction))
            {
                historyCmd.Parameters.AddWithValue("@OrderId", orderId);
                historyCmd.Parameters.AddWithValue("@Status", "Cancelled");
                historyCmd.Parameters.AddWithValue("@ChangedBy", $"User:{userId}");
                await historyCmd.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId)
    {
        var list = new List<OrderStatusHistory>();
        using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();
        var cmd = new SqlCommand(@"
        SELECT HistoryId, OrderId, Status, ChangedAt, ChangedBy
        FROM OrderStatusHistory
        WHERE OrderId = @OrderId
        ORDER BY ChangedAt ASC", con);
        cmd.Parameters.AddWithValue("@OrderId", orderId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new OrderStatusHistory
            {
                HistoryId = (int)reader["HistoryId"],
                OrderId = (int)reader["OrderId"],
                Status = reader["Status"].ToString()!,
                ChangedAt = (DateTime)reader["ChangedAt"],
                ChangedBy = reader["ChangedBy"] as string
            });
        }
        return list;
    }
    public async Task AddOrderStatusHistoryAsync(int orderId, string status, string changedBy)
    {
        using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();
        var cmd = new SqlCommand(@"
        INSERT INTO OrderStatusHistory (OrderId, Status, ChangedBy)
        VALUES (@OrderId, @Status, @ChangedBy)", con);
        cmd.Parameters.AddWithValue("@OrderId", orderId);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@ChangedBy", changedBy);
        await cmd.ExecuteNonQueryAsync();
    }
}