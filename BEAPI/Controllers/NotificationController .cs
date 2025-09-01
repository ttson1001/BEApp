using BEAPI.Dtos.Common;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IUserService _userService;
        public NotificationController(IUserService userService) 
        {
            _userService = userService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendNotificationDto dto)
        {
            await FirebaseNotificationService.SendNotificationAsync(dto.DeviceToken, dto.Title, dto.Body);
            return Ok("✅ Notification sent!");
        }

        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] SendNotificationToUserDto dto)
        {
            try
            {
                await _userService.SendNotificationToUserAsync(dto.UserId, dto.Title, dto.Body);
                return Ok(new { Success = true, Message = "Notification sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }
    }
}
