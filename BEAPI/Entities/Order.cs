using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace BEAPI.Entities
{
    public class Order: BaseEntity
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public Guid CustomerId { get; set; }
        public required User Customer { get; set; }
        public virtual List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
