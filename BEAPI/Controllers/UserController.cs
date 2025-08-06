using BEAPI.Constants;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Elder;
using BEAPI.Dtos.User;
using BEAPI.Exceptions;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

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

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto request)
        {
            try
            {
                await _userService.CreateUserAsync(request);
                return Ok(new ResponseDto { Message = "The user has been created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
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
        public async Task<IActionResult> SearchUsers([FromBody] UserFilterDto request)
        {
            
            try
            {
                var result = await _userService.FilterUsersAsync(request);
                return Ok(new ResponseDto { Data = result, Message = "Filter successful" });
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
