using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAPI.Entities
{
    public class Cart: BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public long Quantity { get; set; }
        public string ProductName { get; set; } = String.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductPrice { get; set; }
        public CartStatus CartStatus { get; set; } = CartStatus.Pending;
        public Guid ElderId { get; set; }
        public virtual User Elder { get; set; }
        public Guid CustomerId { get; set; }
        public virtual User Customer { get; set; }
    }
}
