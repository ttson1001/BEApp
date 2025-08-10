namespace BEAPI.Entities
{
    public class OrderShipmentEvent: BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string Provider { get; set; } = "GHN";
        public string Status { get; set; } = string.Empty; // delivering, delivered, ...
        public string Type { get; set; } = string.Empty;   // create, Switch_status, Update_cod, ...
        public string? Reason { get; set; }
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
