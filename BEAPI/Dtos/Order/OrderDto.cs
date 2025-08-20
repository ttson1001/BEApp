namespace BEAPI.Dtos.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
        public decimal TotalPrice { get; set; }
        public int Discount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? StreetAddress { get; set; }
        public string? WardName { get; set; }
        public string? DistrictName { get; set; }
        public string? ProvinceName { get; set; }

        public string? CustomerName { get; set; }
        public string? ElderName { get; set; }
        public DateTimeOffset? CreationDate { get; set; }

        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }
}
