using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Order
{
    public class CancelOrderResponseDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string CancelReason { get; set; } = string.Empty;
        public OrderStatus PreviousStatus { get; set; }
        public OrderStatus CurrentStatus { get; set; }
        public decimal RefundAmount { get; set; }
        public bool IsRefunded { get; set; }
        public DateTime CancelledAt { get; set; }
    }
}
