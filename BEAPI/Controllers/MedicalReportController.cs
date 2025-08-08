using BEAPI.Dtos.Category;
using BEAPI.Dtos.Common;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalReportController : ControllerBase
    {
        private readonly IMedicalReportService _medicalReportService;

        public MedicalReportController(IMedicalReportService medicalReportService)
        {
            _medicalReportService = medicalReportService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetListValueMedicalReport()
        {
            try
            {
                var rs = await _medicalReportService.GetListValueMedicalReport();
                return Ok(new ResponseDto
                {
                    Message = "MedicalReport getlist successfully",
                    Data = rs,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Data = null,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateListMedicalReport([FromBody] List<CreateCategoryValueDto> dtos)
        {
            try
            {
                await _medicalReportService.CreateListMedicalReport(dtos);
                return Ok(new ResponseDto
                {
                    Message = "MedicalReport created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Data = null,
                    Message = ex.Message
                });
            }
        }
    }
}
