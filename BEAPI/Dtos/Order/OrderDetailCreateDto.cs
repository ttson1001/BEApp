namespace BEAPI.Dtos.Order
{
    public class OrderDetailCreateDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ElderId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
