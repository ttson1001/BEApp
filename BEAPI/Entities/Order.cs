using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace BEAPI.Entities
{
    public class Order: BaseEntity
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public string? Note { get; set; }
        public Guid? ElderId { get; set; }
        public User? Elder { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }
        public string StreetAddress { get; set; } = string.Empty;
        public long WardCode { get; set; }
        public string WardName { get; set; } = string.Empty;
        public int DistrictID { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public virtual List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
