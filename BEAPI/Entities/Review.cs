namespace BEAPI.Entities
{
    public class Review: BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
