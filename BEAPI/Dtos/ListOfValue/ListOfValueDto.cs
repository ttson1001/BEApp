using BEAPI.Dtos.Category;
using BEAPI.Dtos.Value;

namespace BEAPI.Dtos.ListOfValue
{
    public class ListOfValueDto
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        public Entities.Enum.MyValueType Type { get; set; }
        public List<CategoryValueDto> Values { get; set; } = new List<CategoryValueDto>();
    }
}
