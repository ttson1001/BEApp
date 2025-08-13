using BEAPI.Dtos.Common;
using BEAPI.Dtos.Order;
using BEAPI.Entities;
using BEAPI.Services.IServices;
using BEAPI.Helper;
using BEAPI.PaymentService.VnPay;
using BEAPI.Services;
using BEAPI.Services.IServices;
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
                if (string.Equals(type, "WalletTopUp", StringComparison.OrdinalIgnoreCase))
                {
                    var userIdStr = infoDict.GetValueOrDefault("UserId");
                    var amountStr = infoDict.GetValueOrDefault("Amount");
                    var userId = GuidHelper.ParseOrThrow(userIdStr, nameof(userIdStr));
                    _ = decimal.TryParse(amountStr, out var amount);

                    if (vnp_ResponseCode == "00")
                    {
                        await _walletService.TopUp(userId, amount);
                        return Content($@"
            <html>
                <head><meta charset='UTF-8'></head>
                <body>
                    <h2>🎉 Nạp ví thành công!</h2>
                    <p>Số tiền: {amount:n0} VND</p>
                    <a href='http://localhost:3000/'>Quay lại ứng dụng</a>
                </body>
            </html>", "text/html");
                    }
                    return Content($@"
            <html>
                <head><meta charset='UTF-8'></head>
                <body>
                    <h2>❌ Nạp ví thất bại!</h2>
                    <p>Mã lỗi: {vnp_ResponseCode}</p>
                    <a href='http://localhost:3000/'>Thử lại</a>
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

                    await _service.CreateOrderAsync(dto, vnp_ResponseCode == "00");

                    if (vnp_ResponseCode == "00")
                    {
                        return Content($@"
            <html>
                <head><meta charset='UTF-8'></head>
                <body>
                    <h2>🎉 Thanh toán thành công!</h2>
                    <p>Mã giao dịch: {cartId}</p>
                    <a href='http://localhost:3000/'>Quay lại cửa hàng</a>
                </body>
            </html>", "text/html");
                    }
                    return Content($@"
            <html>
                <head><meta charset='UTF-8'></head>
                <body>
                    <h2>❌ Thanh toán thất bại!</h2>
                    <p>Mã lỗi: {vnp_ResponseCode}</p>
                    <a href='http://localhost:3000/'>Thử lại</a>
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
    }
}
