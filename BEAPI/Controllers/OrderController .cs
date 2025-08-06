using BEAPI.Dtos.Common;
using BEAPI.Dtos.Order;
using BEAPI.Entities;
using BEAPI.Helper;
using BEAPI.PaymentService.VnPay;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly VNPayService _vnPayService;

        public OrderController(IOrderService service, VNPayService vnPayService)
        {
            _service = service;
            _vnPayService = vnPayService;
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult VnPay([FromBody] VnPayRequest vnPayRequest)
        {
            var url = _vnPayService.VNPay(HttpContext, vnPayRequest);
            return Ok(new { paymentUrl = url });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Return([FromServices] IOrderService orderService)
        {
            var vnpayData = Request.Query;
            var vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
            var vnp_TxnRef = vnpayData["vnp_OrderInfo"];

            var dto = new OrderCreateDto
            {
                CartId = vnp_TxnRef,
                Note = $"VNPay Transaction: {vnp_TxnRef}"
            };

            await orderService.CreateOrderAsync(dto, vnp_ResponseCode == "00");

            if (vnp_ResponseCode == "00")
            {
                return Content($@"
            <html>
                <head><meta charset='UTF-8'></head>
                <body>
                    <h2>🎉 Thanh toán thành công!</h2>
                    <p>Mã giao dịch: {vnp_TxnRef}</p>
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
