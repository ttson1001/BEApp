using BEAPI.Constants;
using BEAPI.Dtos.Auth;
using BEAPI.Dtos.Common;
using BEAPI.Entities;
using BEAPI.Exceptions;
using BEAPI.Hubs;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IOtpService _otpService;
        private readonly IHubContext<UserOnlineHub> _hubContext;

        public AuthController(IAuthService authService, IOtpService otpService, IHubContext<UserOnlineHub> hubContext)
        {
            _authService = authService;
            _otpService = otpService;
            _hubContext = hubContext;
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
        public async Task<IActionResult> VerifyUserAsync([FromQuery] string otpCode)
        {
            try
            {
                await _authService.VerifyUserAsync(otpCode);
                return Ok(new ResponseDto { Message = "Verify successfully." });
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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
                var (token, user) = await _authService.LoginAsync(dto);
                response.Data = token;
                response.Message = MessageConstants.LoginSuccess;

                await AutoConnectUserToSignalR(user);

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

        private async Task AutoConnectUserToSignalR(User user)
        {
            try
            {
                var userInfo = new UserOnlineHub.UserConnectionInfo
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName ?? user.FullName,
                    ConnectedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow
                };

                UserOnlineHub.AddUserToOnlineList(userInfo);

                await _hubContext.Clients.All.SendAsync("UserOnline", userInfo.UserId, userInfo.UserName);

                Console.WriteLine($"Auto-connected user {userInfo.UserName} ({userInfo.UserId}) to SignalR after login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error auto-connecting user to SignalR: {ex.Message}");
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

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                var userName = User.FindFirst("UserName")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    await AutoDisconnectUserFromSignalR(userId, userName);
                }

                return Ok(new ResponseDto { Message = "Logout successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        private async Task AutoDisconnectUserFromSignalR(string userId, string userName)
        {
            try
            {
                if (UserOnlineHub.IsUserOnline(userId))
                {
                    UserOnlineHub.RemoveUserFromOnlineList(userId);
                    
                    await _hubContext.Clients.All.SendAsync("UserOffline", userId, userName);

                    Console.WriteLine($"Auto-disconnected user {userName} ({userId}) from SignalR after logout");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error auto-disconnecting user from SignalR: {ex.Message}");
            }
        }
    }
}
