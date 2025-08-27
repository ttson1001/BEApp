namespace BEAPI.Dtos.Category
{
    public class UpdateCategoryValueDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
