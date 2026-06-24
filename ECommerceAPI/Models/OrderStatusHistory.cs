namespace ECommerceAPI.Models
{
    public class OrderStatusHistory
    {
        public int HistoryId { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
    }
}
