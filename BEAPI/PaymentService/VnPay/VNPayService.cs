using Microsoft.Extensions.Options;
using BEAPI.Model;

namespace BEAPI.PaymentService.VnPay
{
    public class VNPayService
    {
        private readonly VnPaySettings _settings;

        public VNPayService(IOptions<VnPaySettings> options)
        {
            _settings = options.Value;
        }

        public string VNPay(HttpContext context, VnPayRequest vnPayRequest)
        {
            var createdDate = DateTime.Now;
            var orderId = DateTime.Now.Ticks;

            VnPayLib vnpay = new VnPayLib();

            vnpay.AddRequestData("vnp_Version", VnPayLib.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _settings.TmCode);
            vnpay.AddRequestData("vnp_Amount", (vnPayRequest.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_BankCode", "VNBANK");

            vnpay.AddRequestData("vnp_CreateDate", createdDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + vnPayRequest.CartId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _settings.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());
            vnpay.AddRequestData("cartId", vnPayRequest.CartId.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(_settings.BaseUrl, _settings.HashSecret);
            return paymentUrl;
        }
    }
}
