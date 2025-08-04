using BEAPI.Constants;
using BEAPI.Dtos.Cart;
using BEAPI.Dtos.Common;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Helper;
using BEAPI.Services;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("[action]")]
        public async Task<IActionResult> ReplaceAllCart([FromBody] CartReplaceAllDto dto)
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
            
        [Authorize(Roles = UserContanst.UserRole)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllElderCarts()
        {
            try
            {
                var userId = User.GetUserId();

                var carts = await _service.GetAllElderCarts(userId);
                return Ok(new ResponseDto { Data = carts, Message = "Elder carts retrieved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }


        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetCartById(string id)
        {
            try
            {
                var cart = await _service.GetCartByIdAsync(id);

                if (cart == null)
                {
                    return NotFound(new ResponseDto
                    {
                        Data = null,
                        Message = "Cart not found"
                    });
                }

                return Ok(new ResponseDto
                {
                    Message = "Get cart successfully",
                    Data = cart
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

        [HttpGet("[action]/{customerId}")]
        public async Task<IActionResult> GetCartByCustomerId(string customerId, [FromQuery] CartStatus status)
        {
            try
            {
                var cart = await _service.GetCartByCustomerIdAsync(customerId, status);

                if (cart == null)
                {
                    return NotFound(new ResponseDto
                    {
                        Data = null,
                        Message = "Cart not found"
                    });
                }

                return Ok(new ResponseDto
                {
                    Message = "Get cart successfully",
                    Data = cart
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto
                {
                    Data= null,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id}/[action]")]
        public async Task<IActionResult> ChangeCartStatus(string id, [FromQuery] CartStatus status)
        {
            try
            {
                await _service.ChangeStatus(status, id);

                return Ok(new ResponseDto
                {
                    Data = null,
                    Message = "Cart status updated successfully"
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
