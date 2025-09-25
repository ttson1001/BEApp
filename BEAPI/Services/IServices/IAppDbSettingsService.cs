using BEAPI.Dtos.AppsetiingDto;

namespace BEAPI.Services.IServices
{
    public interface IAppDbSettingsService
    {
        Task<List<AppDbSettingsDto>> GetAllAsync();
        Task<AppDbSettingsDto> GetByIdAsync(Guid id);
        Task<AppDbSettingsDto> CreateAsync(CreateAppDbSettingsDto dto);
        Task EditAsync(UpdateAppDbSettingsDto dto);
        Task DeleteAsync(Guid id);
    }
}
