using BEAPI.Dtos.Category;
using BEAPI.Dtos.ListOfValue;
using BEAPI.Dtos.Value;

namespace BEAPI.Services.IServices
{
    public interface ICategoryService
    {
        Task CreateListOfValueWithValuesAsync(ListOfValueWithValuesCreateDto dto);
        Task<ListOfValueTreeDto> GetListOfValueTreeAsync();
        Task CreateListCategory(List<CreateCategoryValueDto> categoryValueDtos);
        Task LinkSubCategory(LinkCategoryDto linkCategoryDto);
        Task<List<ListOfValueDto>> GetListCategory();
        Task<List<ListOfValueDto>> GetListCategoryNoValue();
        List<CategoryValueLeafWithPathDto> GetLeafNodesWithPaths(ListOfValueTreeDto tree);
        Task<List<CategoryValueDto>> GetListValueCategoryById(string categoryId);
        Task<List<CategoryValueDto>> GetRootListValueCategory();
    }
}
