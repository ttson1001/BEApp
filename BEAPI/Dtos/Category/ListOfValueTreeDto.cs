using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Category
{
    public class ListOfValueTreeDto
    {
        public string ListId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public MyValueType Type { get; set; }
        public List<ValueTreeNodeDto> Values { get; set; } = new();
    }

}
