using BEAPI.Entities.Enum;
using static System.Net.Mime.MediaTypeNames;

namespace BEAPI.Entities
{
    public class Order: BaseEntity
    {
        public Decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List();
    }
}
