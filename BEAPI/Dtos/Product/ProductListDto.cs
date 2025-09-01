using BEAPI.Dtos.Value;

namespace BEAPI.Dtos.Product
{
    public class ProductListDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public bool IsActive {get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public List<ValueDto> Categories { get; set; } = new();
    }
}
