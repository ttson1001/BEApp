using BEAPI.Dtos.Common;
using BEAPI.Entities;
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
            await _service.ConnectAsync(dto.UserId, dto.ChannelName, dto.Type, dto.Token, dto.Consultant);
            return Ok(new ResponseDto { Message = "Connected", Data = null });
        }

        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect([FromQuery] Guid consultantId)
        {
            await _service.DisconnectAsync(consultantId);
            return Ok(new ResponseDto { Message = "Disconnected" });
        }

        [HttpGet("consultant/{consultantId}")]
        public async Task<IActionResult> GetByConsultant(Guid consultantId)
        {
            var connection = await _service.GetByConsultantAsync(consultantId);
            if (connection == null) return NotFound(new ResponseDto { Message = "No connection found" });
            return Ok(new ResponseDto { Data = connection });
        }
    }
}
