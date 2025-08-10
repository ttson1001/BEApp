using BEAPI.Dtos.Common;
using BEAPI.Dtos.Order;
using BEAPI.Entities;
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

        [HttpPost("{orderId:guid}/ship/ghn")]
        public async Task<IActionResult> ShipWithGhn(Guid orderId, [FromServices] BEAPI.Services.Shipping.ShippingService ship)
        {
            try
            {
                await ship.CreateGhnShipmentForOrderAsync(orderId);
                return Ok(new { message = "Created GHN shipment" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{orderId:guid}/shipment")]
        public async Task<IActionResult> GetShipment(Guid orderId, [FromServices] BEAPI.Services.Shipping.ShippingService ship,
                                             [FromServices] BEAPI.Database.BeContext db)
        {
            var order = await ship.GetOrderWithShipmentAsync(orderId);
            if (order == null) return NotFound(new { message = "Order not found" });

            var timeline = order.ShipmentEvents
                .OrderBy(x => x.OccurredAt)
                .Select(x => new { x.Status, x.Type, x.Reason, x.OccurredAt })
                .ToList();

            return Ok(new
            {
                order.Id,
                order.ShippingProvider,
                order.ShippingServiceId,
                order.ShippingFee,
                order.ShippingCode,
                order.ShippingStatus,
                order.ExpectedDeliveryTime,
                timeline
            });
        }

    }
}
