using BEAPI.Dtos.Common;
using BEAPI.Dtos.Feedback;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;

        public FeedbackController(IFeedbackService service)
        {
            _service = service;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var rs = await _service.GetAllAsync();
                return Ok(new ResponseDto { Message = "Feedback getlist successfully", Data = rs });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var rs = await _service.GetByIdAsync(id);
                return Ok(new ResponseDto { Message = "Get feedback by id successfully", Data = rs });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            try
            {
                var rs = await _service.GetByUserIdAsync(userId);
                return Ok(new ResponseDto { Message = "Get feedback by user successfully", Data = rs });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetByAdminId(Guid adminId)
        {
            try
            {
                var rs = await _service.GetByAdminIdAsync(adminId);
                return Ok(new ResponseDto { Message = "Get feedback by admin successfully", Data = rs });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Search([FromBody] FeedbackSearchDto dto)
        {
            try
            {
                var rs = await _service.SearchAsync(dto);
                return Ok(new ResponseDto { Message = "Search feedback successfully", Data = rs });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] FeedbackCreateDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new ResponseDto { Message = "Feedback created successfully", Data = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromBody] FeedbackUpdateDto dto)
        {
            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new ResponseDto { Message = "Feedback updated successfully", Data = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateStatus([FromBody] FeedbackUpdateStatusDto dto)
        {
            try
            {
                await _service.UpdateStatusAsync(dto);
                return Ok(new ResponseDto { Message = "Feedback status updated", Data = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Respond([FromBody] FeedbackRespondDto dto)
        {
            try
            {
                await _service.RespondAsync(dto);
                return Ok(new ResponseDto { Message = "Feedback responded successfully", Data = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message, Data = null });
            }
        }
    }
}
