namespace BEAPI.Dtos.Value
{
    public class ValueTreeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<ValueTreeDto> Children { get; set; } = new();
    }
}
