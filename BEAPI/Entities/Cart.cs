using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAPI.Entities
{
    public class Cart: BaseEntity
    {
        public Guid CustomerId { get; set; }
        public User Customer { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
