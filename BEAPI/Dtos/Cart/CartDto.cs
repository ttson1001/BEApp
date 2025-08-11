namespace BEAPI.Dtos.Cart
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public Guid? ElderId { get; set; }
        public string? ElderName { get; set; }
        public string Status { get; set; } = null!;
        public List<CartItemDto> Items { get; set; } = new();
    }

}
