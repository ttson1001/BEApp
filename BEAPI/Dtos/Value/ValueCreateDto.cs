namespace BEAPI.Dtos.Value
{
    public class ValueCreateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ListOfValueId { get; set; } = string.Empty;
        public string? ChildListOfValueId { get; set; }
    }
}
