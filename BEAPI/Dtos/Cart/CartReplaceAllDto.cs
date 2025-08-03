namespace BEAPI.Dtos.Cart
{
    public class CartReplaceAllDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public List<CartItemReplaceAllDto> Items { get; set; } = new();
    }
}
