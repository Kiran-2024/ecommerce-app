using ECommerceAPI.Data;
using ECommerceApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECommerceApp.Repositories
{
    public interface IAdminOrderRepository
    {
        Task<(List<AdminOrderDto> Orders, int TotalCount)> GetAllOrdersAsync(
            int page, int pageSize, string? status, string? search, DateTime? fromDate, DateTime? toDate);
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, int updatedBy);
    }

    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly DatabaseHelper _db;

        public AdminOrderRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<(List<AdminOrderDto> Orders, int TotalCount)> GetAllOrdersAsync(
            int page, int pageSize, string? status, string? search, DateTime? fromDate, DateTime? toDate)
        {
            var orders = new List<AdminOrderDto>();
            int totalCount = 0;
            int offset = (page - 1) * pageSize;

            var whereConditions = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(status))
            {
                whereConditions.Add("o.OrderStatus = @Status");   // Status → OrderStatus
                parameters.Add(new SqlParameter("@Status", status));
            }

            if (!string.IsNullOrEmpty(search))
            {
                whereConditions.Add("(u.FullName LIKE @Search OR CAST(o.OrderId AS NVARCHAR) LIKE @Search)");
                parameters.Add(new SqlParameter("@Search", $"%{search}%"));
            }

            if (fromDate.HasValue)
            {
                whereConditions.Add("o.CreatedAt >= @FromDate");
                parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                whereConditions.Add("o.CreatedAt <= @ToDate");
                parameters.Add(new SqlParameter("@ToDate", toDate.Value.AddDays(1)));
            }

            string whereClause = whereConditions.Count > 0
                ? "WHERE " + string.Join(" AND ", whereConditions)
                : "";

            string countQuery = $@"
                SELECT COUNT(*) FROM Orders o
                INNER JOIN Users u ON o.UserId = u.UserId
                {whereClause}";

            string dataQuery = $@"
                SELECT 
        o.OrderId, o.UserId, u.FullName, u.Email,
        o.TotalAmount, o.OrderStatus, o.PaymentMethod, o.PaymentStatus,
        o.CreatedAt,
        COUNT(oi.OrderItemId) AS ItemCount
    FROM Orders o
    INNER JOIN Users u ON o.UserId = u.UserId
    LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
    {whereClause}
    GROUP BY o.OrderId, o.UserId, u.FullName, u.Email,
             o.TotalAmount, o.OrderStatus, o.PaymentMethod, o.PaymentStatus,
             o.CreatedAt
    ORDER BY o.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            // Count parameters copy cheyyadam (same list reuse cheyyadam avoid)
            var countParams = parameters.Select(p =>
                new SqlParameter(p.ParameterName, p.Value)).ToList();

            parameters.Add(new SqlParameter("@Offset", offset));
            parameters.Add(new SqlParameter("@PageSize", pageSize));

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // Total count
            using (var cmd = new SqlCommand(countQuery, conn))
            {
                cmd.Parameters.AddRange(countParams.ToArray());
                totalCount = (int)await cmd.ExecuteScalarAsync();
            }

            // Data
            using (var cmd = new SqlCommand(dataQuery, conn))
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(new AdminOrderDto
                    {
                        OrderId = reader.GetInt32("OrderId"),
                        UserId = reader.GetInt32("UserId"),
                        CustomerName = reader.GetString("FullName"),
                        CustomerEmail = reader.GetString("Email"),
                        TotalAmount = reader.GetDecimal("TotalAmount"),
                        Status = reader.GetString("OrderStatus"),   // column name fix
                        PaymentMethod = reader.GetString("PaymentMethod"),
                        PaymentStatus = reader.GetString("PaymentStatus"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        UpdatedAt = null,   // column doesn't exist in table; set null or remove from DTO
                        ItemCount = reader.GetInt32("ItemCount")
                    });
                }
            }

            return (orders, totalCount);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus, int updatedBy)
        {
            string query = @"
                UPDATE Orders 
    SET OrderStatus = @Status
    WHERE OrderId = @OrderId;

    INSERT INTO OrderStatusHistory (OrderId, Status, ChangedAt, ChangedBy)
    VALUES (@OrderId, @Status, GETDATE(), @ChangedBy);";

            var parameters = new[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@Status", newStatus),
                new SqlParameter("@ChangedBy", updatedBy)
            };

            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters);
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}