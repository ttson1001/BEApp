using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.ListOfValue
{
    public class ListOfValueCreateDto
    {
        public string Label { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public Entities.Enum.MyValueType Type { get; set; }
    }
}
