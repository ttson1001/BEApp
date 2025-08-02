using BEAPI.Dtos.Order;

namespace BEAPI.Services.IServices
{
    public interface IOrderService
    {
        Task CreateAsync(OrderCreateDto dto);
    }
}
