namespace BEAPI.Dtos.Value
{
    public class ValueUpdateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ListOfValueId { get; set; } = string.Empty;
    }
}
