using BEAPI.Dtos.Common;
using BEAPI.Dtos.Withdraw;
using BEAPI.Helper;
using BEAPI.Hubs;
using BEAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WithdrawRequestController : ControllerBase
    {
        private readonly IWithdrawRequestService _service;

        public WithdrawRequestController(IWithdrawRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWithdrawRequest([FromBody] CreateWithdrawRequestDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new Exception("User not authenticated"));
                var result = await _service.CreateAsync(dto, userId);
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetMyWithdrawRequests(string id)
        {
            try
            {
                var result = await _service.GetUserRequestsAsync(GuidHelper.ParseOrThrow(id, "User id"));
                return Ok(new ResponseDto{ Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllRequestsAsync()
        {
            try
            {
                var result = await _service.GetAllRequestsAsync();
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ApproveWithdraw([FromQuery] string id)
        {
            try
            {
                var guidId = GuidHelper.ParseOrThrow(id, nameof(id));
                var success = await _service.ApproveAsync(guidId);

                if (!success)
                    return NotFound(new ResponseDto { Message = "Withdraw request not found" });

                return Ok(new { message = "Approved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RejectWithdraw([FromQuery] string id, [FromQuery] string reason)
        {
            try
            {
                var guidId = GuidHelper.ParseOrThrow(id, nameof(id));
                var success = await _service.RejectAsync(guidId, reason);

                if (!success)
                    return NotFound(new ResponseDto { Message = "Withdraw request not found" });

                return Ok(new { message = "Rejected successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

    }
}
