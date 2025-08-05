using BEAPI.Dtos.Category;
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
        public async Task<IActionResult> CreateNewSubCategory([FromBody] ListOfValueWithValuesCreateDto dto)
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
        public async Task<IActionResult> GetListCategory()
        {
            try
            {
                var rs = await _categoryService.GetListCategory();
                return Ok(new ResponseDto
                {
                    Message = "Category getlist successfully",
                    Data = rs
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
        public async Task<IActionResult> GetListCategoryNoValue()
        {
            try
            {
                var rs = await _categoryService.GetListCategoryNoValue();
                return Ok(new ResponseDto
                {
                    Message = "Category getlist successfully",
                    Data = rs
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
        public async Task<IActionResult> GetListValueCategoryById([FromQuery] string id)
        {
            try
            {
                var rs = await _categoryService.GetListValueCategoryById(id);
                return Ok(new ResponseDto
                {
                    Message = "Category getlist successfully",
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
        public async Task<IActionResult> LinkCategoryWithSubCategory([FromBody] LinkCategoryDto dto)
        {
            try
            {
                await _categoryService.LinkSubCategory(dto);
                return Ok(new ResponseDto
                {
                    Message = "Category link successfully"
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
        public async Task<IActionResult> CreateValueOfCategoryRoot([FromBody] List<CreateCategoryValueDto> dtos)
        {
            try
            {
                await _categoryService.CreateListCategory(dtos);
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

        [HttpGet("[action]")]
        public async Task<IActionResult> GetLeafNodesWithPaths()
        {
            try
            {
                var result = _categoryService.GetLeafNodesWithPaths(await _categoryService.GetListOfValueTreeAsync());
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