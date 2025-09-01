using BEAPI.Dtos.Common;
using BEAPI.Dtos.Review;
using BEAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
        {
            try
            {

                await _reviewService.AddReviewAsync(dto.CustomerId, dto);

                return Ok(new ResponseDto
                {
                    Message = "Review added successfully",
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

        [HttpGet("[action]/{orderId}")]
        public async Task<IActionResult> GetReview(Guid orderId)
        {
            var reviews = await _reviewService.GetOrdertReviewsAsync(orderId);
            return Ok(new ResponseDto
            {
                Message = "Success",
                Data = reviews
            });
        }
    }

}
