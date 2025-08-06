using BEAPI.Dtos.Category;

namespace BEAPI.Services.IServices
{
    public interface IRelationShipService
    {
        Task CreateListRelationship(List<CreateCategoryValueDto> categoryValueDtos);
        Task<List<CategoryValueDto>> GetListValueRelationship();
    }
}
