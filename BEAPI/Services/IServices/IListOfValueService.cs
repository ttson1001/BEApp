using BEAPI.Dtos.ListOfValue;

namespace BEAPI.Services.IServices
{
    public interface IListOfValueService
    {
        Task Create(ListOfValueCreateDto listOfValueCreateDto);
        Task Update(ListOfValueUpdateDto listOfValueUpdateDto);
        Task<List<ListOfValueDto>> GetAllAsync();
        Task<ListOfValueDto> GetByIdAsync(string id);
        Task<List<ListOfValueDto>> GetByNoteAsync(string note);
    }
}
