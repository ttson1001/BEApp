using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Helper;
using BEAPI.Model;
using BEAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace BEAPI.PaymentService.VnPay
{
    public class VNPayService
    {
        private readonly VnPaySettings _settings;
        private readonly IRepository<Cart> _repository;

        public VNPayService(IOptions<VnPaySettings> options, IRepository<Cart> repository)
        {
            _settings = options.Value;
            _repository = repository;
        }

        public string VNPay(HttpContext context, VnPayRequest vnPayRequest)
        {
            var createdDate = DateTime.Now;
            var orderId = DateTime.Now.Ticks;
            var cartId = GuidHelper.ParseOrThrow(vnPayRequest.CartId, nameof(vnPayRequest.CartId));
            var cart = ValidateCart(cartId);
            var total = cart.Items.Sum(i => i.ProductPrice);
            var orderInfo = $"CartId={vnPayRequest.CartId};AddressId={vnPayRequest.AddressId};Note={vnPayRequest.Note}";
            VnPayLib vnpay = new VnPayLib();

            vnpay.AddRequestData("vnp_Version", VnPayLib.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _settings.TmCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(total * 100)).ToString());
            vnpay.AddRequestData("vnp_BankCode", "VNBANK");

            vnpay.AddRequestData("vnp_CreateDate", createdDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", WebUtility.UrlEncode(orderInfo));
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _settings.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(_settings.BaseUrl, _settings.HashSecret);
            return paymentUrl;
        }

        private Cart ValidateCart(Guid cartId)
        {
            var cart =  _repository.Get()
                .Include(x => x.Items)
                    .ThenInclude(x => x.ProductVariant)
                        .ThenInclude(x => x.Product)
                .FirstOrDefault(u => u.Id == cartId && u.Status == CartStatus.Pending)
                ?? throw new Exception("Cart not found or not in pending status");

            if (!cart.Items.Any())
                throw new Exception("Cart is empty");

            if (cart.Items.Any(item => item.ProductVariant == null || item.ProductVariant.Product == null))
                throw new Exception("Invalid product or variant in cart item");

            if (cart.Items.Any(item => item.Quantity <= 0))
                throw new Exception("Invalid quantity for item");

            if (cart.Items.Any(item => item.ProductVariant.Stock < item.Quantity))
            {
                var outOfStockItem = cart.Items.First(item => item.ProductVariant.Stock < item.Quantity);
                var productName = outOfStockItem.ProductVariant.Product?.Name ?? "Unknown";
                throw new Exception($"Not enough stock for product: {productName}");
            }

            return cart;
        }
    }
}
