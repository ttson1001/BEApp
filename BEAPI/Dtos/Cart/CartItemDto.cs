namespace BEAPI.Dtos.Cart
{
    public class CartItemDto
    {
        public Guid ProductVariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }
        public string Styles { get; set; }
        public string ImageUrl { get; set; }
        public int Discount { get; set; }

    }
}
