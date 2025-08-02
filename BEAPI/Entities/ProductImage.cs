namespace BEAPI.Entities
{
    public class ProductImage: BaseEntity
    {
        public string URL { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
