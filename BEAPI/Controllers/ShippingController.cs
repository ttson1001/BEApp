using BEAPI.Services.Shipping;
using BEAPI.Dtos.Common;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly ShippingService _ship;

        public ShippingController(ShippingService ship)
        {
            _ship = ship;
        }

        //[HttpPost("orders/{orderId:guid}/quote-default")]
        //public async Task<IActionResult> QuoteDefault(Guid orderId, CancellationToken ct)
        //{
        //    var (serviceId, serviceTypeId, fee) = await _ship.QuoteFeeDefaultAsync(orderId, ct);
        //    return Ok(new { serviceId, serviceTypeId, fee });
        //}

        //[HttpPost("orders/{orderId:guid}/recalc-fee-default")]
        //public async Task<IActionResult> RecalcFeeDefault(Guid orderId, CancellationToken ct)
        //{
        //    var (serviceId, serviceTypeId, fee) = await _ship.RecalcAndSaveFeeDefaultAsync(orderId, ct);
        //    return Ok(new { serviceId, serviceTypeId, fee, saved = true });
        //}

        [HttpPost("{orderId:guid}/[action]")]
        public async Task<IActionResult> CreateOrderInGHN(Guid orderId, CancellationToken ct)
        {
            try
            {
                var (code, etd, serviceId, serviceTypeId, fee) = await _ship.CreateGhnShipmentDefaultAsync(orderId, ct);
                return Ok(new ResponseDto 
                { 
                    Message = "Create order in GHN successfully",
                    Data = new { code, etd, serviceId, serviceTypeId, fee }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        //[HttpPost("orders/{orderId:guid}/pickup")]
        //public async Task<IActionResult> Pickup(Guid orderId, [FromQuery] DateTimeOffset? pickupAt, CancellationToken ct)
        //{
        //    await _ship.RequestPickupAsync(orderId, pickupAt, ct);
        //    return Ok(new { message = "Pickup requested" });
        //}

        //[HttpPost("orders/{orderId:guid}/sync")]
        //public async Task<IActionResult> Sync(Guid orderId, CancellationToken ct)
        //{
        //    var status = await _ship.SyncShipmentStatusAsync(orderId, ct);
        //    return Ok(new { status });
        //}

        //[HttpPost("orders/{orderId:guid}/cancel")]
        //public async Task<IActionResult> Cancel(Guid orderId, [FromQuery] string? note, CancellationToken ct)
        //{
        //    await _ship.CancelShipmentAsync(orderId, note, ct);
        //    return Ok(new { message = "Cancelled" });
        //}


        [HttpPost("{orderId:guid}/[action]")]
        public async Task<IActionResult> FakeGHNChangeStatus(Guid orderId, CancellationToken ct)
        {
            try
            {
                var next = await _ship.AdvanceOrderOnlyAsync(orderId, ct);
                return Ok(new ResponseDto 
                { 
                    Message = "Change status successfully", 
                    Data = new { orderStatus = next.ToString() }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }
    }
}
