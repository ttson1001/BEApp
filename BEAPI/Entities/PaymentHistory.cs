using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAPI.Entities
{
    public class PaymentHistory: BaseEntity
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string? PaymentMenthod { get; set; }
        public PaymentStatus paymentStatus { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}
