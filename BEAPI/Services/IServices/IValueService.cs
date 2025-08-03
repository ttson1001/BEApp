using BEAPI.Dtos.Value;

namespace BEAPI.Services.IServices
{
    public interface IValueService
    {
        Task Create(ValueCreateDto valueCreateDto);
        Task Update(ValueUpdateDto valueUpdateDto);
        Task<List<ValueDto>> GetValuesByListIdAsync(string listOfValueId);
        Task<List<ValueDto>> GetValuesByNoteAsync(string note);
        Task<List<ValueDto>> GetAllValuesAsync();
        Task<ValueTreeDto> GetValueWithChildrenAsync(Guid valueId);
    }
}
