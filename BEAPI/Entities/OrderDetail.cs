using System;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace BEAPI.Entities
{
    public class OrderDetail: BaseEntity
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public Guid ProductVariantId { get; set; }
        public required ProductVariant ProductVariant { get; set; }
    }
}
