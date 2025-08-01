namespace BEAPI.Entities
{
    public class ProductVariantValue: BaseEntity
    {
        public Guid VauleId { get ; set; }
        public Value Value { get; set; }
        public Guid ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}
