using BEAPI.Dtos.Cart;
using BEAPI.Entities;
using BEAPI.Entities.Enum;

namespace BEAPI.Services.IServices
{
    public interface ICartService
    {
        Task ReplaceCartAsync(CartReplaceAllDto dto);
        Task<List<CartDto>> GetAllElderCarts(string userId);
        Task<CartDto?> GetCartByIdAsync(string id);
        Task ChangeStatus(CartStatus cartStatus, string id);
        Task<CartDto?> GetCartByCustomerIdAsync(string cusId, CartStatus cartStatus);
    }
}
