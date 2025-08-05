using BEAPI.Dtos.Category;

namespace BEAPI.Services.IServices
{
    public interface IBrandService
    {
        Task CreateListBrand(List<CreateCategoryValueDto> categoryValueDtos);
        Task<List<CategoryValueDto>> GetListValueBrand();
    }
}
