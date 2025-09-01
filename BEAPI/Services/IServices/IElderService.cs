using BEAPI.Dtos.Addreess;
using BEAPI.Dtos.Category;
using BEAPI.Dtos.Elder;

namespace BEAPI.Services.IServices
{
    public interface IElderService
    {
        Task<List<ElderDto>> GetElderByCusId(string cusId);
        Task UpdateElderAsync(ElderUpdateDto dto);
        Task ChangeIsDeletedAsync(string id);
        Task UpdateElderAdressAsync(List<UpdateAddressDto> addressesDto, string userId);
        Task UpdateElderCategory(List<UpdateCategoryElderDto> updateCategoryElderDtos, Guid elderId);
        Task<ElderFinanceDto> GetElderFinanceAsync(Guid elderId);
    }
}
