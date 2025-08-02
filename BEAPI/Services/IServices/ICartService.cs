using BEAPI.Dtos.Cart;
using BEAPI.Entities;

namespace BEAPI.Services.IServices
{
    public interface ICartService
    {
        Task ReplaceCartAsync(CartUpdateDto dto);
    }
}
