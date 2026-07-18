namespace ECommerceAPI.DTO_s
{
    public class SalesReportDto
    {
        public DateTime OrderDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrderValue { get; set; }
    }

    public class CategoryRevenueDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class OrderStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}