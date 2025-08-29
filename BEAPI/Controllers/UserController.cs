using BEAPI.Constants;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Elder;
using BEAPI.Dtos.User;
using BEAPI.Helper;
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
        //[Authorize(Roles = UserContanst.UserRole)]
        public async Task<IActionResult> GenerateQr([FromBody] ElderQrDto dto)
        {
            try
            {
                var result = await _userService.GenerateElderLoginQrAsync(GuidHelper.ParseOrThrow(dto.ElderId));
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

        [HttpPut("{userId}/[action]")]
        public async Task<IActionResult> BanOrUnbanUser(string userId)
        {
            try
            {
                await _userService.BanOrUnbanUserAsync(userId);
                return Ok(new ResponseDto
                {
                    Message = "User status updated successfully.",
                    Data = null
                });
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
        public async Task<IActionResult> QrLogin([FromQuery] string token, [FromQuery] string? deviceId)
        {
            try
            {
                var newToken = await _userService.LoginByQrAsync(token, deviceId);

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

        [HttpGet("[action]")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            try
            {
                var rs = await _userService.GetDetailAsync(id);
                return Ok(new ResponseDto
                {
                    Message = "Get user detail successfully",
                    Data = rs
                });
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

        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromBody] UserUpdateDto dto)
        {
            try
            {
                await _userService.UpdateUserAsync(dto);
                return Ok(new ResponseDto
                {
                    Message = "User updated successfully",
                    Data = null
                });
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
    }
}
