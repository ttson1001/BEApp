using BEAPI.Constants;
using BEAPI.Dtos.Auth;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Order;
using BEAPI.Exceptions;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public TestController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto orderCreateDto)
        {
            try
            {
                await _orderService.CreateOrderAsync(orderCreateDto , true);
                return Ok(new ResponseDto { Message = "Create dto successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

    }
}
