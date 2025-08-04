namespace BEAPI.Services.IServices
{
    public interface ILocationService
    {
        Task SyncProvincesAsync();
        Task SyncDistrictsAsync();
        Task SyncAllWardsAsync(List<int> districtIds);
        Task SyncAllAsync();
    }
}
