using BEAPI.Dtos.Common;
using BEAPI.Dtos.Elder;
using BEAPI.Dtos.User;
using BEAPI.Helper;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElderController : ControllerBase
    {
        private readonly IElderService _elderService;

        public ElderController(IElderService elderService)
        {
            _elderService = elderService;
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetElderByCusId()
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
