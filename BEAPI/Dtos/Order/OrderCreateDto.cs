namespace BEAPI.Dtos.Order
{
    public class OrderCreateDto
    {
        public string CartId { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
