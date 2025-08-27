using BEAPI.Entities.Enum;

namespace BEAPI.Entities
{
    public class WithdrawRequest : BaseEntity
    {
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal Amount { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public WithdrawStatus Status { get; set; }
    }
}
