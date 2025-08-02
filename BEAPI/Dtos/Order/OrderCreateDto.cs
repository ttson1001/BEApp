namespace BEAPI.Dtos.Order
{
    public class OrderCreateDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public List<OrderDetailCreateDto> OrderDetails { get; set; } = new();

    }
}
