namespace BEAPI.Dtos.Product
{
    public class ProductVariantCreateDto
    {
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public long Stock { get; set; }
        public bool IsActive { get; set; } = true;
        public List<ProductImageCreateDto> ProductImages { get; set; } = new();
        public List<string> ValueIds { get; set; } = new();
    }
}
