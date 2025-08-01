namespace BEAPI.Entities
{
    public class ProductVariant: BaseEntity
    {
        public Decimal Price { get; set; }
        public int Discount { get; set; }
        public long Stock { get; set;}
        public bool IsActive { get; set; } = true;
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
