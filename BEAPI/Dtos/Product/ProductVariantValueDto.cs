namespace BEAPI.Dtos.Product
{
    public class ProductVariantValueDto
    {
        public string Id { get; set; } = string.Empty;
        public string ValueId { get; set; } = string.Empty;
        public string? ValueCode { get; set; }
        public string? ValueLabel { get; set; }
    }
}
