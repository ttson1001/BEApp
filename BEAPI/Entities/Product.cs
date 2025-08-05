namespace BEAPI.Entities
{
    public class Product: BaseEntity
    {
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public DateTimeOffset? ManufactureDate { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public string? VideoPath { get; set; }
        public string? Description { get; set; }
        public int? Weight { get; set; }
        public int? Height { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public List<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        public List<ProductCategoryValue> ProductCategoryValues { get; set; } = new List<ProductCategoryValue>();
    }
}
