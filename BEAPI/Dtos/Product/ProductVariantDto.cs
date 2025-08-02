namespace BEAPI.Dtos.Product
{
    public class ProductVariantDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public long Stock { get; set; }
        public bool IsActive { get; set; }
        public List<ProductVariantValueDto> ProductVariantValues { get; set; } = new();
    }
}
