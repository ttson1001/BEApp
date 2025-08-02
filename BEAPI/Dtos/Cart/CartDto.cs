namespace BEAPI.Dtos.Cart
{
    public class CartDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.ProductPrice * i.Quantity);
    }

}
