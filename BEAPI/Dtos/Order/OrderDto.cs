namespace BEAPI.Dtos.Order
{
    public class OrderDto
    {
        public string Id { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Note { get; set; }
        public int OrderStatus { get; set; }
        public string ElderName { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
    }
}
