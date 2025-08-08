namespace BEAPI.Dtos.Product
{
    public class ProductAttributeGroupDto
    {
        public Guid ListOfValueId { get; set; }
        public string Label { get; set; } = string.Empty;
        public List<ProductAttributeValueDto> Options { get; set; } = new();
    }
}
