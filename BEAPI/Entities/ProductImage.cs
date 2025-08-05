namespace BEAPI.Entities
{
    public class ProductImage: BaseEntity
    {
        public string URL { get; set; }
        public Guid ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}
