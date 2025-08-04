using BEAPI.Dtos.Order;

namespace BEAPI.Services.IServices
{
    public interface IOrderService
    {
        Task CreateOrderAsync(OrderCreateDto dto, bool isPaid);
    }
}
