using BEAPI.Dtos.Common;
using BEAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserConnectionController : ControllerBase
    {
        private readonly IUserConnectionService _service;

        public UserConnectionController(IUserConnectionService service)
        {
            _service = service;
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect([FromBody] ConnectDto dto)
        {
            try
            {
                //await _service.ConnectAsync(dto.UserId, dto.ChannelName, dto.Type, dto.Token, dto.ProductId, dto.Consultant);
                await _service.ConnectAsync(dto.UserId, dto.ChannelName, dto.Type, dto.Token, null, dto.Consultant);
                return Ok(new ResponseDto { Message = "Connected", Data = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect([FromQuery] Guid consultantId)
        {
            try
            {
                await _service.DisconnectAsync(consultantId);
                return Ok(new ResponseDto { Message = "Disconnected" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UserDisconnect([FromQuery] Guid userId)
        {
            try
            {
                await _service.UserDisconnectAsync(userId);
                return Ok(new ResponseDto { Message = "Disconnected" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UserAcceptAsync([FromQuery] Guid userId)
        {
            try
            {
                await _service.UserAcceptAsync(userId);
                return Ok(new ResponseDto { Message = "Disconnected" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("consultant/{consultantId}")]
        public async Task<IActionResult> GetByConsultant(Guid consultantId)
        {
            try
            {
                var connection = await _service.GetByConsultantAsync(consultantId);
                if (connection == null)
                    return NotFound(new ResponseDto { Message = "No connection found" });

                return Ok(new ResponseDto { Data = connection });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("{consultantId}/decline")]
        public async Task<IActionResult> Decline(Guid consultantId)
        {
            try
            {
                await _service.Decline(consultantId);

                return Ok(new ResponseDto
                {
                    Message = "Consultant declined successfully",
                    Data = new { DeclinedConsultantId = consultantId }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }
    }
}
