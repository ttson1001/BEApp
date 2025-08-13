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
    }
}


