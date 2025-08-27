using BEAPI.Dtos.Category;
using BEAPI.Dtos.Common;
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

        [HttpPut("[action]")]
        public async Task<IActionResult> DeactivateOrActiveBrand([FromQuery] Guid valueId)
        {
            try
            {
                await _brandService.DeactivateOrActiveBrandAsync(valueId);
                return Ok(new ResponseDto
                {
                    Message = "Brand Deactivate Or Active successfully",
                    Data = null,
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

        [HttpPut("[action]")]
        public async Task<IActionResult> EditBrand([FromQuery] UpdateCategoryValueDto updateCategoryValueDto)
        {
            try
            {
                await _brandService.EditBrandAsync(updateCategoryValueDto);
                return Ok(new ResponseDto
                {
                    Message = "Brand Edit successfully",
                    Data = null,
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
