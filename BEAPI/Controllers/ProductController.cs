using BEAPI.Dtos.Common;
using BEAPI.Dtos.Product;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            try
            {
                await _service.Create(dto);
                return Ok(new ResponseDto
                {
                    Message = "Product created successfully",
                    Data = null
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAll();
                return Ok(new ResponseDto
                {
                    Message = "Get all products successfully",
                    Data = result
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

        [HttpGet("[action]/{productId}")]
        public async Task<IActionResult> GetById(string productId)
        {
            try
            {
                var result = await _service.GetById(productId);
                return Ok(new ResponseDto
                {
                    Message = "Get product successfully",
                    Data = result
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
    }

}
