namespace BEAPI.PaymentService.VnPay
{
    public class VnPayRequest
    {
        public string CartId { get; set; }
        public string AddressId { get; set; }
        public string Note { get; set; }
        public string? UserPromotionId { get; set; }
    }
}
