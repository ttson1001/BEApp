using BEAPI.Entities;

namespace BEAPI.Services.IServices
{
    public interface IinternalLocationService
    {
        Task<List<Province>> GetProvincesAsync();
        Task<List<District>> GetDistrictsByProvinceAsync(int provinceId);
        Task<List<Ward>> GetWardsByDistrictAsync(int districtId);
    }
}
