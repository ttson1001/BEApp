namespace BEAPI.Dtos.Cart
{
    public class CartItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public long Quantity { get; set; }
        public string ElderId { get; set; } = string.Empty;
    }
}
