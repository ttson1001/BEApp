namespace BEAPI.Services.IServices
{
    using BEAPI.Dtos.Common;
    using BEAPI.Dtos.Promotion;

    public interface IPromotionService
    {
        Task<List<PromotionDto>> GetAllAsync();
        Task<PromotionDto> GetByIdAsync(Guid id);
        Task<PagedResult<PromotionDto>> SearchAsync(PromotionSearchDto dto);

        Task CreateAsync(PromotionCreateDto dto);
        Task UpdateAsync(PromotionUpdateDto dto);
        Task DeleteAsync(Guid id);

        Task RedeemAsync(RedeemPromotionDto dto);
        Task<List<PromotionDto>> GetMyPromotionsAsync(Guid userId, bool onlyAvailable);
    }

}
