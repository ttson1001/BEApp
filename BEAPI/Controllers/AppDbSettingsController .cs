using BEAPI.Dtos.AppsetiingDto;
using BEAPI.Dtos.Common;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppDbSettingsController : ControllerBase
    {
        private readonly IAppDbSettingsService _service;

        public AppDbSettingsController(IAppDbSettingsService service)
        {
            _service = service;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new ResponseDto { Data = result, Message = "Get all settings successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(new ResponseDto { Data = result, Message = "Get setting successfully" });
            }
            catch (Exception ex)
            {
                return NotFound(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateAppDbSettingsDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return StatusCode(StatusCodes.Status201Created,
                    new ResponseDto { Data = result, Message = "Setting created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Edit([FromBody] UpdateAppDbSettingsDto dto)
        {
            try
            {
                await _service.EditAsync(dto);
                return Ok(new ResponseDto { Message = "Setting updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new ResponseDto { Message = "Setting deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }
    }
}
