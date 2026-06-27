using ECommerceAPI.DTOs;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ECommerceAPI.Repositories
{
    public interface IAdminRepository
    {
        Task<AdminDashboardDto> GetDashboardStatsAsync();
    }

    public class AdminRepository : IAdminRepository
    {
        private readonly string _connectionString;

        public AdminRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var dashboard = new AdminDashboardDto();

            // 1. Aggregate stats
            var statsQuery = @"
                SELECT
                    COUNT(*) AS TotalOrders,
                    ISNULL(SUM(TotalAmount), 0) AS TotalRevenue,
                    SUM(CASE WHEN OrderStatus = 'Pending' THEN 1 ELSE 0 END) AS PendingOrders,
                    SUM(CASE WHEN OrderStatus = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledOrders
                FROM Orders";

            using (var cmd = new SqlCommand(statsQuery, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    dashboard.TotalOrders = reader.GetInt32("TotalOrders");
                    dashboard.TotalRevenue = reader.GetDecimal("TotalRevenue");
                    dashboard.PendingOrders = reader.GetInt32("PendingOrders");
                    dashboard.CancelledOrders = reader.GetInt32("CancelledOrders");
                }
            }

            // 2. Total Users
            var usersQuery = "SELECT COUNT(*) FROM Users WHERE IsActive = 1";
            using (var cmd = new SqlCommand(usersQuery, conn))
            {
                dashboard.TotalUsers = (int)await cmd.ExecuteScalarAsync();
            }

            // 3. Total Products
            var productsQuery = "SELECT COUNT(*) FROM Products WHERE IsActive = 1";
            using (var cmd = new SqlCommand(productsQuery, conn))
            {
                dashboard.TotalProducts = (int)await cmd.ExecuteScalarAsync();
            }

            // 4. Recent 5 Orders
            var recentQuery = @"
                SELECT TOP 5 o.OrderId, u.FullName AS CustomerName,
                       o.TotalAmount, o.OrderStatus, o.CreatedAt
                FROM Orders o
                JOIN Users u ON o.UserId = u.UserId
                ORDER BY o.CreatedAt DESC";

            using (var cmd = new SqlCommand(recentQuery, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    dashboard.RecentOrders.Add(new RecentOrderDto
                    {
                        OrderId = reader.GetInt32("OrderId"),
                        CustomerName = reader.GetString("CustomerName"),
                        TotalAmount = reader.GetDecimal("TotalAmount"),
                        OrderStatus = reader.GetString("OrderStatus"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    });
                }
            }

            // 5. Low Stock Products (Stock < 10)
            var lowStockQuery = @"
                SELECT TOP 10 ProductId, ProductName, Stock
                FROM Products
                WHERE IsActive = 1 AND Stock < 10
                ORDER BY Stock ASC";

            using (var cmd = new SqlCommand(lowStockQuery, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    dashboard.LowStockProducts.Add(new LowStockProductDto
                    {
                        ProductId = reader.GetInt32("ProductId"),
                        ProductName = reader.GetString("ProductName"),
                        Stock = reader.GetInt32("Stock")
                    });
                }
            }

            return dashboard;
        }
    }
}