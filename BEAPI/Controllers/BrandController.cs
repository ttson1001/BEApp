using BEAPI.Dtos.Category;
using BEAPI.Dtos.Common;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController: ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetListValueBrand()
        {
            try
            {
                var rs = await _brandService.GetListValueBrand();
                return Ok(new ResponseDto
                {
                    Message = "Brand getlist successfully",
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
        public async Task<IActionResult> CreateValueOfBrandRoot([FromBody] List<CreateCategoryValueDto> dtos)
        {
            try
            {
                await _brandService.CreateListBrand(dtos);
                return Ok(new ResponseDto
                {
                    Message = "Brand created successfully"
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
