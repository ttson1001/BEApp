using BEAPI.Dtos.Category;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.ListOfValue;
using BEAPI.Dtos.Value;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductPropertyController: ControllerBase
    {
        private readonly IProductPropertySerivce _productPropertySerivce;


        public ProductPropertyController(IProductPropertySerivce productPropertySerivce)
        {
            _productPropertySerivce = productPropertySerivce;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetListProductProperty()
        {
            try
            {
                var rs = await _productPropertySerivce.GetListProductProperty();
                return Ok(new ResponseDto
                {
                    Message = "Property getlist successfully",
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

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllValueProductProperty()
        {
            try
            {
                var rs = await _productPropertySerivce.GetAllValueProductProperty();
                return Ok(new ResponseDto
                {
                    Message = "Productproperty getlist successfully",
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
        public async Task<IActionResult> CreateListOfValueWithValuesAsync([FromBody] ListOfValueWithValuesCreateDto dtos)
        {
            try
            {
                await _productPropertySerivce.CreateListOfValueWithValuesAsync(dtos);
                return Ok(new ResponseDto
                {
                    Message = "Property created successfully"
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
        public async Task<IActionResult> DeactivateOrActiveProductProperty([FromQuery] Guid valueId)
        {
            try
            {
                await _productPropertySerivce.DeactivateOrActiveProductPropertyAsync(valueId);
                return Ok(new ResponseDto
                {
                    Message = "ProductProperty Deactivate Or Active successfully",
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
        public async Task<IActionResult> EditProductProperty([FromQuery] UpdateCategoryValueDto updateCategoryValueDto)
        {
            try
            {
                await _productPropertySerivce.EditProductPropertyAsync(updateCategoryValueDto);
                return Ok(new ResponseDto
                {
                    Message = "ProductProperty Edit successfully",
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
    }
}
