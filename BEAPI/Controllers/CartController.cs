using BEAPI.Dtos.Cart;
using BEAPI.Dtos.Common;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;

        public CartController(ICartService service)
        {
            _service = service;
        }

        [HttpPost("replace")]
        public async Task<IActionResult> ReplaceCart([FromBody] CartUpdateDto dto)
        {
            try
            {
                await _service.ReplaceCartAsync(dto);
                return Ok(new ResponseDto
                {
                    Message = "Cart updated successfully",
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

    }
}
