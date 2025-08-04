using BEAPI.Constants;
using BEAPI.Dtos.Auth;
using BEAPI.Dtos.Common;
using BEAPI.Exceptions;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [Authorize(Roles = UserContanst.UserRole)]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateElder([FromBody] ElderRegisterDto dto)
        {
            var res = new ResponseDto();
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null) {
                res.Message = ExceptionConstant.UserIdMissing;
                return BadRequest(res);
            }
            try
            {
                await _userService.CreateElder(dto,Guid.Parse(userId.ToString()));
                res.Message = MessageConstants.RegisterSuccess;
                return StatusCode(StatusCodes.Status201Created, res);
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                if (ex.Message == ExceptionConstant.UserAlreadyExists)
                    return Conflict(res);

                return BadRequest(res);
            }
        }

        [HttpPost("[action]")]
        [Authorize(Roles = UserContanst.UserRole)]
        public async Task<IActionResult> GenerateQr([FromBody] Guid elderId)
        {
            try
            {
                var result = await _userService.GenerateElderLoginQrAsync(elderId);
                var response = new ResponseDto
                {
                    Message = "QR code generated successfully",
                    Data = new
                    {
                        QrCode = result.QrBase64,
                        result.Token
                    }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> QrLogin([FromQuery] string token)
        {
            try
            {
                var newToken = await _userService.LoginByQrAsync(token);

                var response = new ResponseDto
                {
                    Message = "Login successful",
                    Data = new { Token = newToken }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new ResponseDto
                {
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
