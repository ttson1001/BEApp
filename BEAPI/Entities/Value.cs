namespace BEAPI.Entities
{
    public class Value : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public Guid ListOfValueId { get; set; }
        public ListOfValue? ListOfValue { get; set; }
    }
}
