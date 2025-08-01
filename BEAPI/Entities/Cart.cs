using BEAPI.Entities.Enum;

namespace BEAPI.Entities
{
    public class Cart: BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public long Quantity { get; set; }
        public string ProductName { get; set; } = String.Empty;
        public Decimal ProductPrice { get; set; }
        public CartStatus CartStatus { get; set; } = CartStatus.Pending;
    }
}
