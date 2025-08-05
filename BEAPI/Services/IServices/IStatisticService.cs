using BEAPI.Dtos.Statistic;
using BEAPI.Entities.Enum;

namespace BEAPI.Services.IServices
{
    public interface IStatisticService
    {
        Task<List<StatisticCountDto>> GetTotalOrdersAsync(TimeScope timeFrame, DateTime? chosenDate);
        Task<List<StatisticCountDto>> GetTotalProductsAsync(TimeScope timeFrame, DateTime? chosenDate);
        Task<List<StatisticCountDto>> GetTotalCustomersAsync(TimeScope timeFrame, DateTime? chosenDate);
        Task<List<StatisticCountDto>> GetTotalRevenuesAsync(TimeScope timeFrame, DateTime? chosenDate);
        Task<TopNProductStatisticDto> GetTopNProductsAsync(int topN);
        Task<TopNCustomerStatisticDto> GetTopNCustomersAsync(int topN);
        Task<Object> GetCurrentStatisticsAsync();
    }
}
