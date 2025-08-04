using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAPI.Entities
{
    public class Cart: BaseEntity
    {
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }
        public Guid? ElderId { get; set; }
        public User? Elder { get; set; }
        public CartStatus Status { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
