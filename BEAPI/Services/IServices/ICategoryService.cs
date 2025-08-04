using BEAPI.Dtos.Category;
using BEAPI.Dtos.Value;

namespace BEAPI.Services.IServices
{
    public interface ICategoryService
    {
        Task CreateListOfValueWithValuesAsync(ListOfValueWithValuesCreateDto dto);
        Task<ListOfValueTreeDto> GetListOfValueTreeAsync();
    }
}
