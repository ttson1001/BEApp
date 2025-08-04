using BEAPI.Dtos.Common;
using BEAPI.Dtos.Value;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] ListOfValueWithValuesCreateDto dto)
        {
            try
            {
                await _categoryService.CreateListOfValueWithValuesAsync(dto);
                return Ok(new ResponseDto
                {
                    Message = "Category created successfully"
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

        [HttpGet("[action]")]
        public async Task<IActionResult> GetTree()
        {
            try
            {
                var result = await _categoryService.GetListOfValueTreeAsync();
                return Ok(new ResponseDto
                {
                    Data = result,
                    Message = "Get Category with tree successfully"
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