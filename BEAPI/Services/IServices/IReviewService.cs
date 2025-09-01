using BEAPI.Dtos.Review;
using BEAPI.Entities;

namespace BEAPI.Services.IServices
{
    public interface IReviewService
    {
        Task AddReviewAsync(Guid userId, CreateReviewDto dto);
        Task<ReviewDto?> GetOrdertReviewsAsync(Guid orderId);
    }
}
