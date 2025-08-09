using BEAPI.Dtos.Common;
using BEAPI.Dtos.Report;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var rs = await _service.GetAllAsync();
                return Ok(new ResponseDto
                {
                    Message = "Report getlist successfully",
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

        [HttpGet("[action]")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var rs = await _service.GetByIdAsync(id);
                return Ok(new ResponseDto
                {
                    Message = "Get report by id successfully",
                    Data = rs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            try
            {
                var rs = await _service.GetByUserIdAsync(userId);
                return Ok(new ResponseDto
                {
                    Message = "Get report by userId successfully",
                    Data = rs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetByConsultantId(Guid consultantId)
        {
            try
            {
                var rs = await _service.GetByConsultantIdAsync(consultantId);
                return Ok(new ResponseDto
                {
                    Message = "Get report by consultantId successfully",
                    Data = rs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetByUserAndConsultant(Guid userId, Guid consultantId)
        {
            try
            {
                var rs = await _service.GetByUserAndConsultantAsync(userId, consultantId);
                return Ok(new ResponseDto
                {
                    Message = "Get report by user & consultant successfully",
                    Data = rs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Search([FromBody] ReportSearchDto dto)
        {
            try
            {
                var rs = await _service.SearchAsync(dto);
                return Ok(new ResponseDto
                {
                    Message = "Search report successfully",
                    Data = rs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] ReportCreateDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new ResponseDto
                {
                    Message = "Report created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromBody] ReportUpdateDto dto)
        {
            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new ResponseDto
                {
                    Message = "Report updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Message = ex.Message
                });
            }
        }
    }
}
