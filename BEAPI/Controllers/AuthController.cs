using BEAPI.Constants;
using BEAPI.Dtos.Auth;
using BEAPI.Dtos.Common;
using BEAPI.Exceptions;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
  
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
