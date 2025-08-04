using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Category
{
    public class ValueTreeNodeDto
    {
        public string ValueId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MyValueType Type { get; set; }
        public List<ListOfValueTreeDto> Children { get; set; } = new();
    }
}
