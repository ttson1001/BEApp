using BEAPI.Dtos.Category;
using BEAPI.Dtos.ListOfValue;
using BEAPI.Dtos.Value;

namespace BEAPI.Services.IServices
{
    public interface IProductPropertySerivce
    {
        Task CreateListOfValueWithValuesAsync(ListOfValueWithValuesCreateDto dto);

        Task<List<ListOfValueDto>> GetListProductProperty();

        Task<List<ValueDto>> GetAllValueProductProperty();

        Task DeactivateOrActiveProductPropertyAsync(Guid valueId);

        Task EditProductPropertyAsync(UpdateCategoryValueDto dto);
    }
}
