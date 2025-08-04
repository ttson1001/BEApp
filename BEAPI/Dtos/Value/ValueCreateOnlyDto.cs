using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Value
{
    public class ValueCreateOnlyDto
    {
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MyValueType Type { get; set; }
    }
}
