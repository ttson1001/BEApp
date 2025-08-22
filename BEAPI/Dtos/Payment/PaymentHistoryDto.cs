using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Payment
{
    public class PaymentHistoryDto
    {
        public string Id { get; set; }
        public decimal? Amount { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
        public string? PaymentMenthod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTimeOffset? CreationDate { get; set; }
        public string? OrderId { get; set; }
    }
}


