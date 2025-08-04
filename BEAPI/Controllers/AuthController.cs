using BEAPI.Constants;
using BEAPI.Dtos.Auth;
using BEAPI.Dtos.Common;
using BEAPI.Entities;
using BEAPI.Exceptions;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IOtpService _otpService;

        public AuthController(IAuthService authService, IOtpService otpService)
        {
            _authService = authService;
            _otpService = otpService;
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request);
                return Ok(new ResponseDto { Message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPDto request)
        {
            User user;
            try
            {
                user = await _authService.FindUserByEmailOrPhoneAsync(request.EmailOrPhone);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }

            await _otpService.GenerateAndSendOtpAsync(user);
            return Ok(new ResponseDto { Message = "OTP sent." });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var response = new ResponseDto();
            try
            {
                await _authService.RegisterAsync(dto);
                response.Message = MessageConstants.RegisterSuccess;
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;

                if (ex.Message == ExceptionConstant.UserAlreadyExists)
                    return Conflict(response);

                return BadRequest(response);
            }
        }

        [Authorize]
        [HttpPut("[action]")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ResponseDto { Message = "UserId not found in token" });

                await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);

                return Ok(new ResponseDto { Message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {

            var response = new ResponseDto();
            try
            {
                var token = await _authService.LoginAsync(dto);
                response.Data = token;
                response.Message = MessageConstants.LoginSuccess;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                if (ex.Message == ExceptionConstant.InvalidCredentials)
                    return Unauthorized(response);

                return BadRequest(response);
            }
        }

        [Authorize]
        [HttpGet("[action]")]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = User.FindFirst("UserId")?.Value,
                userName = User.FindFirst("UserName")?.Value,
                role = User.FindFirst("Role")?.Value
            });
        }
    }
}
