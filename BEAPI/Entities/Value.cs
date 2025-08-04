using BEAPI.Entities.Enum;

namespace BEAPI.Entities
{
    public class Value : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public Enum.MyValueType Type { get; set; } = Enum.MyValueType.ProductProperty;
        public Guid ListOfValueId { get; set; }
        public ListOfValue? ListOfValue { get; set; }
        public Guid? ChildListOfValueId { get; set; }
        public ListOfValue? ChildListOfValue { get; set; }
        public virtual List<ProductVariantValue> productVariantValues { get; set; } = new List<ProductVariantValue>();
        public virtual List<ProductCategoryValue> ProductCategories { get; set; } = new List<ProductCategoryValue>();
    }
}
