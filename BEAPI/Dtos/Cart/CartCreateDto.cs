namespace BEAPI.Dtos.Cart
{
    public class CartCreateDto
    {
        public string ProductId { get; set; } = string.Empty;
        public long Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string ElderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
    }
}
