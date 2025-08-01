namespace BEAPI.Entities
{
    public class ListOfValue : BaseEntity
    {
        public string Label { get; set; } =  string.Empty;
        public string Note { get; set; } = string.Empty;

        public ICollection<Value> Values { get; set; } = new List<Value>();
    }
}
