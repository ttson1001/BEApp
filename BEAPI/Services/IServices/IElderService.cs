using BEAPI.Dtos.Elder;

namespace BEAPI.Services.IServices
{
    public interface IElderService
    {
        Task<List<ElderDto>> GetElderByCusId(string cusId);
        Task UpdateElderAsync(ElderUpdateDto dto);
        Task ChangeIsDeletedAsync(string id);
    }
}
