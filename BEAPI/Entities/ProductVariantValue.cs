namespace BEAPI.Entities
{
    public class ProductVariantValue: BaseEntity
    {
        public Guid ValueId { get ; set; }
        public Value Value { get; set; }
        public Guid ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}
