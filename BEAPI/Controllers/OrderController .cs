using BEAPI.Dtos.Order;
using BEAPI.Services.IServices;
using BEAPI.Helper;
using BEAPI.PaymentService.VnPay;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly VNPayService _vnPayService;
        private readonly IWalletService _walletService;

        public OrderController(IOrderService service, VNPayService vnPayService, IWalletService walletService)
        {
            _service = service;
            _vnPayService = vnPayService;
            _walletService = walletService;
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult VnPay([FromBody] VnPayRequest vnPayRequest)
        {
            try
            {
                var result = _vnPayService.VNPay(HttpContext, vnPayRequest);
                return Ok(new { message = "Vnpay successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Return()
        {
            try
            {
                var vnpayData = Request.Query;
                var vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
                var vnp_OrderInfo = vnpayData["vnp_OrderInfo"]; 
                var orderInfoDecoded = WebUtility.UrlDecode(vnp_OrderInfo);

                var infoDict = orderInfoDecoded.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split('='))
                    .Where(x => x.Length == 2)
                    .ToDictionary(x => x[0], x => x[1]);

                var type = infoDict.GetValueOrDefault("Type");
                var status = vnp_ResponseCode == "00" ? "success" : (vnp_ResponseCode == "24" ? "cancel" : "fail");
                if (string.Equals(type, "WalletTopUp", StringComparison.OrdinalIgnoreCase))
                {
                    var userIdStr = infoDict.GetValueOrDefault("UserId");
                    var amountStr = infoDict.GetValueOrDefault("Amount");
                    var userId = GuidHelper.ParseOrThrow(userIdStr, nameof(userIdStr));
                    _ = decimal.TryParse(amountStr, out var amount);

                    if (status == "success")
                    {
                        await _walletService.TopUp(userId, amount);
                    }
                    var deepLink = $"silvercart://payment/callback?status={status}";
                    return Content($@"
            <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='refresh' content='0;url={deepLink}'>
                    <script>window.location.href='{deepLink}';</script>
                </head>
                <body>
                    <p>Redirecting...</p>
                    <a href='{deepLink}'>Tap here if not redirected</a>
                </body>
            </html>", "text/html");
                }
                else
                {
                    var cartId = infoDict.GetValueOrDefault("CartId");
                    var addressId = infoDict.GetValueOrDefault("AddressId");
                    var note = infoDict.GetValueOrDefault("Note");
                    var dto = new OrderCreateDto
                    {
                        CartId = cartId,
                        AddressId = addressId,
                        Note = note
                    };

                    await _service.CreateOrderAsync(dto, status == "success");
                    var deepLink = $"silvercart://payment/callback?status={status}";
                    return Content($@"
            <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='refresh' content='0;url={deepLink}'>
                    <script>window.location.href='{deepLink}';</script>
                </head>
                <body>
                    <p>Redirecting...</p>
                    <a href='{deepLink}'>Tap here if not redirected</a>
                </body>
            </html>", "text/html");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CheckoutByWallet([FromBody] OrderCreateDto dto)
        {
            try
            {
                await _service.CreateOrderByWalletAsync(dto);
                return Ok(new { message = "Checkout by wallet successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetOrdersByUser()
        {
            try
            {
                var userId = User.GetUserId();
                var orders = await _service.GetOrdersByCustomerIdAsync(userId);
                return Ok(new { message = "Get order successfully", data = orders });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SearchOrders([FromBody] OrderFilterDto request)
        {
            try
            {
                var result = await _service.FilterOrdersAsync(request);
                return Ok(new { message = "Get order successfully", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var order = await _service.GetOrderByIdAsync(id);
                return Ok(new { message = "Get order successfully", data = order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
