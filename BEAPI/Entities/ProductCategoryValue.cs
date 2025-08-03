namespace BEAPI.Entities
{
    public class ProductCategoryValue: BaseEntity
    {
        public Guid ValueId { get; set; }
        public Value Value { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
