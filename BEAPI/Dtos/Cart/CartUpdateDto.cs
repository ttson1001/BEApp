namespace BEAPI.Dtos.Cart
{
    public class CartUpdateDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public List<CartItemCreateDto> Items { get; set; } = new();
    }
}
