namespace BEAPI.Dtos.Product
{
    public class ProductCreateDto
    {
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? Descriotion { get; set; }
        public string? VideoPath { get; set; }
        public int? Weight { get; set; }
        public int? Height { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public DateTimeOffset? ManufactureDate { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public List<string> ValueCategoryIds { get; set; } = new();
        public List<ProductImageCreateDto> ProductImages { get; set; } = new();
        public List<ProductVariantCreateDto> ProductVariants { get; set; } = new();
    }
}
