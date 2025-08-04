namespace BEAPI.Dtos.Value
{
    public class ListOfValueWithValuesCreateDto
    {
        public string Label { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public ValueType Type { get; set; }
        public List<ValueCreateOnlyDto> Values { get; set; } = new();
    }
}

