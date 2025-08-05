using System.ComponentModel.DataAnnotations.Schema;

namespace BEAPI.Entities
{
    public class ProductVariant: BaseEntity
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public long Stock { get; set;}
        public bool IsActive { get; set; } = true;
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public List<ProductVariantValue> ProductVariantValues { get; set; } = new List<ProductVariantValue>();
    }
}
