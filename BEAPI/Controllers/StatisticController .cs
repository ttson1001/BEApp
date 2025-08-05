using BEAPI.Dtos;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Statistic;
using BEAPI.Entities.Enum;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> TotalOrders([FromQuery] TimeScope timeScope, [FromQuery] DateTime? chosenDate)
        {
            try
            {
                var result = await _statisticService.GetTotalOrdersAsync(timeScope, chosenDate);
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> TotalProducts([FromQuery] TimeScope timeScope, [FromQuery] DateTime? chosenDate)
        {
            try
            {
                var result = await _statisticService.GetTotalProductsAsync(timeScope, chosenDate);
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> TotalCustomers([FromQuery] TimeScope timeScope, [FromQuery] DateTime? chosenDate)
        {
            try
            {
                var result = await _statisticService.GetTotalCustomersAsync(timeScope, chosenDate);
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> TotalRevenues([FromQuery] TimeScope timeScope, [FromQuery] DateTime? chosenDate)
        {
            try
            {
                var result = await _statisticService.GetTotalRevenuesAsync(timeScope, chosenDate);
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> TopNProducts([FromQuery] int topN = 5)
        {
            try
            {
                var result = await _statisticService.GetTopNProductsAsync(topN);
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> TopNCustomers([FromQuery] int topN = 5)
        {
            try
            {
                var result = await _statisticService.GetTopNCustomersAsync(topN);
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CurrentStatistics()
        {
            try
            {
                var result = await _statisticService.GetCurrentStatisticsAsync();
                return Ok(new ResponseDto { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }
    }
}
