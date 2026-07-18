// Repositories/AdminReportsRepository.cs
using System.Data;
using ECommerceAPI.DTO_s;
using Microsoft.Data.SqlClient;

public class AdminReportsRepository
{
    private readonly string _connectionString;

    public AdminReportsRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    // Default to last 30 days if no dates passed
    private (DateTime start, DateTime end) ResolveDates(DateTime? start, DateTime? end)
    {
        var resolvedEnd = end ?? DateTime.Today;
        var resolvedStart = start ?? resolvedEnd.AddDays(-30);
        return (resolvedStart, resolvedEnd);
    }

    public List<SalesReportDto> GetSalesReport(DateTime? startDate, DateTime? endDate)
    {
        var (start, end) = ResolveDates(startDate, endDate);
        var result = new List<SalesReportDto>();

        const string query = @"
            SELECT 
                CAST(o.CreatedAt AS DATE) AS OrderDate,
                COUNT(DISTINCT o.OrderId) AS TotalOrders,
                SUM(o.TotalAmount) AS TotalRevenue,
                AVG(o.TotalAmount) AS AvgOrderValue
            FROM Orders o
            WHERE o.CreatedAt >= @StartDate 
              AND o.CreatedAt < DATEADD(DAY, 1, @EndDate)
              AND o.OrderStatus <> 'Cancelled'
            GROUP BY CAST(o.CreatedAt AS DATE)
            ORDER BY CAST(o.CreatedAt AS DATE);";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@StartDate", start);
        cmd.Parameters.AddWithValue("@EndDate", end);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new SalesReportDto
            {
                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders")),
                TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue")),
                AvgOrderValue = reader.GetDecimal(reader.GetOrdinal("AvgOrderValue"))
            });
        }
        return result;
    }

    public List<CategoryRevenueDto> GetRevenueByCategory(DateTime? startDate, DateTime? endDate)
    {
        var (start, end) = ResolveDates(startDate, endDate);
        var result = new List<CategoryRevenueDto>();

        const string query = @"
            SELECT 
                c.CategoryId,
                c.CategoryName,
                COUNT(DISTINCT oi.OrderId) AS TotalOrders,
                SUM(oi.Quantity) AS UnitsSold,
                SUM(oi.Quantity * oi.UnitPrice) AS TotalRevenue
            FROM OrderItems oi
            INNER JOIN Products p ON oi.ProductId = p.ProductId
            INNER JOIN Categories c ON p.CategoryId = c.CategoryId
            INNER JOIN Orders o ON oi.OrderId = o.OrderId
            WHERE o.CreatedAt >= @StartDate 
              AND o.CreatedAt < DATEADD(DAY, 1, @EndDate)
              AND o.OrderStatus <> 'Cancelled'
            GROUP BY c.CategoryId, c.CategoryName
            ORDER BY TotalRevenue DESC;";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@StartDate", start);
        cmd.Parameters.AddWithValue("@EndDate", end);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new CategoryRevenueDto
            {
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders")),
                UnitsSold = reader.GetInt32(reader.GetOrdinal("UnitsSold")),
                TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"))
            });
        }
        return result;
    }

    public List<OrderStatusSummaryDto> GetOrderStatusSummary(DateTime? startDate, DateTime? endDate)
    {
        var (start, end) = ResolveDates(startDate, endDate);
        var result = new List<OrderStatusSummaryDto>();

        const string query = @"
            SELECT 
               o.OrderStatus AS Status,
                COUNT(*) AS OrderCount,
                SUM(o.TotalAmount) AS TotalAmount
            FROM Orders o
            WHERE o.CreatedAt >= @StartDate 
              AND o.CreatedAt < DATEADD(DAY, 1, @EndDate)
            GROUP BY o.OrderStatus
            ORDER BY OrderCount DESC;";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@StartDate", start);
        cmd.Parameters.AddWithValue("@EndDate", end);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new OrderStatusSummaryDto
            {
                Status = reader.GetString(reader.GetOrdinal("Status")),
                OrderCount = reader.GetInt32(reader.GetOrdinal("OrderCount")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
            });
        }
        return result;
    }
}