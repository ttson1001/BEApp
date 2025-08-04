namespace BEAPI.PaymentService.VnPay
{
    public class VnPayRequest
    {
        public string CartId { get; set; }
        public decimal Amount { get; set; }
    }
}
