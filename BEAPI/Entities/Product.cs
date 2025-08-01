namespace BEAPI.Entities
{
    public class Product: BaseEntity
    {
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public Guid ProductTypeId { get; set; }
        public DateTimeOffset? ManufactureDate { get; set; }
        public DateTimeOffset? DateTimeOffset { get; set; }
        public string? VideoPath { get; set; }
        public string? Descriotion { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? Length { get; set; }
        public string? Width { get; set; }
        public List<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    }
}
