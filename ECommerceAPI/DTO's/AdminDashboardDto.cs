namespace ECommerceAPI.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}