namespace BEAPI.Dtos.Cart
{
    public class CartItemCreateDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ElderId { get; set; } = string.Empty;
        public long Quantity { get; set; }
    }
}
