using BEAPI.Dtos.Common;
using BEAPI.Dtos.Promotion;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _service;

        public PromotionController(IPromotionService service)
        {
            _service = service;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var rs = await _service.GetAllAsync();
                return Ok(new ResponseDto { Message = "Promotion getlist successfully", Data = rs });
            }
            catch (Exception ex) { return BadRequest(new ResponseDto { Message = ex.Message }); }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var rs = await _service.GetByIdAsync(id);
                return Ok(new ResponseDto { Message = "Get promotion by id successfully", Data = rs });
            }
            catch (Exception ex) { return BadRequest(new ResponseDto { Message = ex.Message }); }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Search([FromBody] PromotionSearchDto dto)
        {
            try
            {
                var rs = await _service.SearchAsync(dto);
                return Ok(new ResponseDto { Message = "Search promotion successfully", Data = rs });
            }
            catch (Exception ex) { return BadRequest(new ResponseDto { Message = ex.Message }); }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] PromotionCreateDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new ResponseDto { Message = "Promotion created successfully" });
            }
            catch (Exception ex) { return BadRequest(new ResponseDto { Message = ex.Message }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Update([FromBody] PromotionUpdateDto dto)
        {
            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new ResponseDto { Message = "Promotion updated successfully" });
            }
            catch (Exception ex) { return BadRequest(new ResponseDto { Message = ex.Message }); }
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new ResponseDto { Message = "Promotion deleted successfully" });
            }
            catch (Exception ex) { return BadRequest(new ResponseDto { Message = ex.Message }); }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Redeem([FromBody] RedeemPromotionDto dto)
        {
            try
            {
                await _service.RedeemAsync(dto);
                return Ok(new ResponseDto { Message = "Redeem promotion successfully" });
            }
            catch (Exception ex) { return BadRequest(new ResponseDto { Message = ex.Message }); }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> MyPromotions(Guid userId, bool onlyAvailable = true)
        {
            try
            {
                var rs = await _service.GetMyPromotionsAsync(userId, onlyAvailable);
                return Ok(new ResponseDto { Message = "Get my promotions successfully", Data = rs });
            }
            catch (Exception ex) { return BadRequest(new ResponseDto { Message = ex.Message }); }
        }
    }
}
