using BEAPI.Constants;
using BEAPI.Dtos.Addreess;
using BEAPI.Dtos.Category;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Elder;
using BEAPI.Exceptions;
using BEAPI.Helper;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElderController : ControllerBase
    {
        private readonly IElderService _elderService;
        private readonly IUserService _userService;

        public ElderController(IElderService elderService, IUserService userService)
        {
            _elderService = elderService;
            _userService = userService;
        }

        [Authorize(Roles = UserContanst.UserRole)]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateElder([FromBody] ElderRegisterDto dto)
        {
            var res = new ResponseDto();
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                res.Message = ExceptionConstant.UserIdMissing;
                return BadRequest(res);
            }
            try
            {
                await _userService.CreateElder(dto, Guid.Parse(userId.ToString()));
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

        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetMyElders()
        {
            try
            {
                var userId = User.GetUserId();
                var rs = await _elderService.GetElderByCusId(userId);
                return Ok(new ResponseDto { Data = rs, Message = "Get elder successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]/{elderId}")]
        public async Task<IActionResult> GetElderFinanceAsync(Guid elderId)
        {
            try
            {
                var rs = await _elderService.GetElderFinanceAsync(elderId);
                return Ok(new ResponseDto { Data = rs, Message = "Get elder finance successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateElder([FromBody] ElderUpdateDto dto)
        {
            try
            {
                await _elderService.UpdateElderAsync(dto);
                return Ok(new ResponseDto {Message = "Update elder successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateElderAdressAsync([FromBody] List<UpdateAddressDto> dto, string elderId)
        {
            try
            {
                await _elderService.UpdateElderAdressAsync(dto,elderId);
                return Ok(new ResponseDto { Message = "Update elderAddress successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateElderCategory([FromBody] List<UpdateCategoryElderDto> dto, Guid elderId)
        {
            try
            {   
                await _elderService.UpdateElderCategory(dto, elderId);
                return Ok(new ResponseDto { Message = "Update elderAddress successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPatch("[action]/{id}")]
        public async Task<IActionResult> ChangeStatus([FromRoute] string id)
        {
            try
            {
                await _elderService.ChangeIsDeletedAsync(id);
                return Ok(new ResponseDto { Message = "Change status elder successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
