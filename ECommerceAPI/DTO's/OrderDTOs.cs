namespace ECommerceAPI.DTO_s
{
    public class CreateOrderDto
    {
        public int? AddressId { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int? AddressId { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? DeliveryAddress { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; } = new();
    }

    public class OrderItemResponseDto
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
    public class OrderStatusHistoryDto
    {
        public int HistoryId { get; set; }
        public string Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
    }
}
