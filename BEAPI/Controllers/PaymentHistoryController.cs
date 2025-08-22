using BEAPI.Dtos.Common;
using BEAPI.Dtos.Payment;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentHistoryController : ControllerBase
    {
        private readonly IPaymentHistoryService _service;

        public PaymentHistoryController(IPaymentHistoryService service)
        {
            _service = service;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Search([FromBody] PaymentHistorySearchDto request)
        {
            try
            {
                var result = await _service.SearchAsync(request);
                return Ok(new ResponseDto { Message = "Search payment history successfully", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken ct)
        {
            try
            {
                var payments = await _service.GetByUserIdAsync(userId, ct);

                return Ok(new ResponseDto
                {
                    Data = payments,
                    Message = "payment history successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Data = null,
                    Message = ex.Message
                });
            }
        }
    }
}


