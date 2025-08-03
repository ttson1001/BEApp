namespace BEAPI.Dtos.Order
{
    public class OrderDetailCreateDto
    {
        public string ProductVariantId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
