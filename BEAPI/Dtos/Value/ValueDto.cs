namespace BEAPI.Dtos.Value
{
    public class ValueDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public Entities.Enum.MyValueType Type { get; set; }
        public string ListOfValueId { get; set; } = string.Empty;
    }
}

