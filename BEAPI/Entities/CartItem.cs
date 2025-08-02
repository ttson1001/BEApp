using System.ComponentModel.DataAnnotations.Schema;

namespace BEAPI.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Guid ElderId { get; set; }
        public User Elder { get; set; } = null!;

        public long Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductPrice { get; set; }
    }
}
