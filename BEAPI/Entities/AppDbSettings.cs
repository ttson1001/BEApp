namespace BEAPI.Entities
{
    public class AppDbSettings : BaseEntity
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
