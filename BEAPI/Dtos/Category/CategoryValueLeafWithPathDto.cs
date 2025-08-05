using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Category
{
    public class CategoryValueLeafWithPathDto
    {
        public string ValueId { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public MyValueType Type { get; set; }
        public List<string> Path { get; set; }
    }
}
