using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Helper;
using BEAPI.Model;
using BEAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Globalization;

namespace BEAPI.PaymentService.VnPay
{
    public class VNPayService
    {
        private readonly IRepository<UserPromotion> _userPromotionRepo;
        private readonly VnPaySettings _settings;
        private readonly IRepository<Cart> _repository;

        public VNPayService(IOptions<VnPaySettings> options, IRepository<UserPromotion> userPromotionRepo, IRepository<Cart> repository)
        {
            _settings = options.Value;
            _repository = repository;
            _userPromotionRepo = userPromotionRepo;
        }

        public string VNPay(HttpContext context, VnPayRequest vnPayRequest)
        {
            var createdDate = DateTime.Now;
            var orderId = DateTime.Now.Ticks;

            var cartId = GuidHelper.ParseOrThrow(vnPayRequest.CartId, nameof(vnPayRequest.CartId));
            var cart = ValidateCart(cartId);

            var subTotal = cart.Items.Sum(i => i.ProductPrice * i.Quantity);

            decimal itemDiscountAmount = cart.Items.Sum(i =>
            {
                var percent = i.Discount > 0 ? i.Discount : i.ProductVariant.Discount;
                return (decimal)percent / 100m * i.ProductPrice * i.Quantity;
            });

            var userPromotion = ValidateAndGetPromotion(vnPayRequest.UserPromotionId, cart.CustomerId);

            decimal baseAfterItemDiscount = subTotal - itemDiscountAmount;
            if (baseAfterItemDiscount < 0) baseAfterItemDiscount = 0;

            int promoPercent = userPromotion?.Promotion?.DiscountPercent ?? 0;
            decimal promoDiscountAmount = (decimal)promoPercent / 100m * baseAfterItemDiscount;

            decimal total = baseAfterItemDiscount - promoDiscountAmount;
            if (total < 0) total = 0;

            long vnpAmount = (long)Math.Round(total * 100m, MidpointRounding.AwayFromZero);

            var orderInfo = $"CartId={vnPayRequest.CartId};AddressId={vnPayRequest.AddressId};Note={vnPayRequest.Note};UserPromotionId={vnPayRequest.UserPromotionId}";

            VnPayLib vnpay = new VnPayLib();
            vnpay.AddRequestData("vnp_Version", VnPayLib.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _settings.TmCode);
            vnpay.AddRequestData("vnp_Amount", vnpAmount.ToString());
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

        public string VNPayWalletTopUp(HttpContext context, Guid userId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new Exception("Amount must be greater than 0");
            }
            var createdDate = DateTime.Now;
            var orderId = DateTime.Now.Ticks;

            long vnpAmount = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
            var orderInfo = $"Type=WalletTopUp;UserId={userId};Amount={amount.ToString(CultureInfo.InvariantCulture)}";

            VnPayLib vnpay = new VnPayLib();
            vnpay.AddRequestData("vnp_Version", VnPayLib.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _settings.TmCode);
            vnpay.AddRequestData("vnp_Amount", vnpAmount.ToString());
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

        private UserPromotion? ValidateAndGetPromotion(string? userPromotionId, Guid customerId)
        {
            if (string.IsNullOrWhiteSpace(userPromotionId) || !Guid.TryParse(userPromotionId, out var upId))
                return null;

            var userPromotion = _userPromotionRepo.Get()
                .Include(up => up.Promotion)
                .FirstOrDefault(up => up.Id == upId)
                ?? throw new Exception("UserPromotion not found");

            if (userPromotion.UserId != customerId)
                throw new Exception("Promotion does not belong to this user");
            if (userPromotion.IsUsed)
                throw new Exception("Promotion was already used");

            var now = DateTimeOffset.UtcNow;
            var promo = userPromotion.Promotion;
            if (!promo.IsActive) throw new Exception("Promotion is inactive");
            if (promo.StartAt.HasValue && promo.StartAt.Value > now) throw new Exception("Promotion not started yet");
            if (promo.EndAt.HasValue && promo.EndAt.Value < now) throw new Exception("Promotion expired");

            return userPromotion;
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
