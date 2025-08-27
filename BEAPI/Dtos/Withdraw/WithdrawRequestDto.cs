using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Withdraw
{
    public class WithdrawRequestDto
    {
        public Guid Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal Amount { get; set; }
        public WithdrawStatus Status { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
