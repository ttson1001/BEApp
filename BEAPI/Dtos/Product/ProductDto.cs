using BEAPI.Dtos.Value;

namespace BEAPI.Dtos.Product
{
    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? VideoPath { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? Length { get; set; }
        public string? Width { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? ManufactureDate { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public List<ValueDto> Categories { get; set; } = new();
        public List<ProductVariantDto> ProductVariants { get; set; } = new();
    }
}
