using BEAPI.Dtos.Common;
using BEAPI.Helper;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgoraController : ControllerBase
    {
        private readonly IAgoraService _agoraService;

        public AgoraController(IAgoraService agoraService)
        {
            _agoraService = agoraService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateChannel([FromQuery] string callerId, [FromQuery] string calleeId)
        {
            try
            {
                var caller = GuidHelper.ParseOrThrow(callerId, nameof(callerId));
                var callee = GuidHelper.ParseOrThrow(calleeId, nameof(calleeId));
                var (appId, channel, callerUid, callerToken, calleeUid, calleeToken) = await _agoraService.CreateChannelAsync(caller, callee);
                return Ok(new ResponseDto
                {
                    Message = "Agora channel created",
                    Data = new { appId, channel, callerUid, callerToken, calleeUid, calleeToken }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetJoinInfo([FromQuery] string callerId, [FromQuery] string calleeId, [FromQuery] string asRole)
        {
            try
            {
                var caller = GuidHelper.ParseOrThrow(callerId, nameof(callerId));
                var callee = GuidHelper.ParseOrThrow(calleeId, nameof(calleeId));
                var (appId, channel, callerUid, callerToken, calleeUid, calleeToken) = await _agoraService.CreateChannelAsync(caller, callee);

                var isCaller = string.Equals(asRole, "caller", StringComparison.OrdinalIgnoreCase);
                var uid = isCaller ? callerUid : calleeUid;
                var token = isCaller ? callerToken : calleeToken;

                return Ok(new ResponseDto
                {
                    Message = "Agora join info",
                    Data = new { appId, channel, uid, token }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }
    }
}


