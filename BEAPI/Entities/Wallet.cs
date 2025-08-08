using System.ComponentModel.DataAnnotations.Schema;

namespace BEAPI.Entities
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; set; }

        public User User { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }
}
