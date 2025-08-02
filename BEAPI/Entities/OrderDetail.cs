using System;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace BEAPI.Entities
{
    public class OrderDetail: BaseEntity
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public string Note { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public int Weight { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Guid ProductId { get; set; }
        public required Product Product { get; set; }

        public Guid ElderId { get; set; }
        public required User Elder { get; set; }

    }
}
