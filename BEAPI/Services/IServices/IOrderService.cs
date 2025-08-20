using BEAPI.Dtos.Common;
using BEAPI.Dtos.Order;

namespace BEAPI.Services.IServices
{
    public interface IOrderService
    {
        Task CreateOrderAsync(OrderCreateDto dto, bool isPaid);
        Task CreateOrderByWalletAsync(OrderCreateDto dto);
        Task<List<OrderDto>> GetOrdersByCustomerIdAsync(string userId);
        Task<List<OrderDto>> GetOrdersByElderIdAsync(string userId);
        Task<PagedResult<OrderDto>> FilterOrdersAsync(OrderFilterDto request);
        Task<OrderDto?> GetOrderByIdAsync(string orderId);
        Task<OrderStatisticDto> UserStatistic(Guid UserId);
        Task<List<ElderBudgetStatisticDto>> ElderBudgetStatistic(Guid customerId, DateTime fromDate, DateTime toDate);
    }
}
