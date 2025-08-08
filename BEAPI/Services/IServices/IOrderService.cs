using BEAPI.Dtos.Common;
using BEAPI.Dtos.Order;

namespace BEAPI.Services.IServices
{
    public interface IOrderService
    {
        Task CreateOrderAsync(OrderCreateDto dto, bool isPaid);
        Task<List<OrderDto>> GetOrdersByCustomerIdAsync(string userId);
        Task<PagedResult<OrderDto>> FilterOrdersAsync(OrderFilterDto request);
    }
}
