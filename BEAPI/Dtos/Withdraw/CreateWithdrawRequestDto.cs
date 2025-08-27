namespace BEAPI.Dtos.Withdraw
{
    public class CreateWithdrawRequestDto
    {
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string? Note { get; set; }

        public decimal Amount { get; set; }
    }
}
