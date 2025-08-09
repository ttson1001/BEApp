using BEAPI.Dtos.Common;
using BEAPI.Dtos.Feedback;

namespace BEAPI.Services.IServices
{
    public interface IFeedbackService
    {
        Task<FeedbackDto> GetByIdAsync(Guid id);
        Task<List<FeedbackDto>> GetAllAsync();
        Task<List<FeedbackDto>> GetByUserIdAsync(Guid userId);
        Task<List<FeedbackDto>> GetByAdminIdAsync(Guid adminId);
        Task<PagedResult<FeedbackDto>> SearchAsync(FeedbackSearchDto dto);

        Task CreateAsync(FeedbackCreateDto dto);
        Task UpdateAsync(FeedbackUpdateDto dto);
        Task UpdateStatusAsync(FeedbackUpdateStatusDto dto);
        Task RespondAsync(FeedbackRespondDto dto);
    }
}
