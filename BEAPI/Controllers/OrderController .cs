using BEAPI.Dtos.Common;
using BEAPI.Dtos.Order;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateElderOrderAsync([FromBody] OrderCreateDto dto)
        {
            try
            {
                await _service.CreateElderOrderAsync(dto);
                return Ok(new ResponseDto
                {
                    Message = "Order created successfully",
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
