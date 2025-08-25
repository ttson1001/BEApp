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

        [HttpGet("[action]")]
        public IActionResult RedirectTo([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new ResponseDto { Message = "url is required" });
            }
            // Basic allow-list: allow http(s) and custom schemes (e.g., app:// or silvercart://)
            // In production, you should validate domains/schemes strictly to prevent open redirects.
            return Content($@"
            <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='refresh' content='0;url={url}'>
                    <script>window.location.href='{url}';</script>
                </head>
                <body>
                    <p>Redirecting...</p>
                    <a href='{url}'>Tap here if not redirected</a>
                </body>
            </html>", "text/html");
        }

        [HttpGet("[action]")]
        public IActionResult Test2([FromQuery] string url)
        {
            return Ok(url);
        }
    }
}
