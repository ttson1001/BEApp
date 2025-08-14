namespace BEAPI.Dtos.Wallet
{
    public class WalletAmountDto
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
    }

    public class GetWalletAmountDto
    {
        public string UserId { get; set; }
    }
}


