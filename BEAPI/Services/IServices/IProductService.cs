using BEAPI.Dtos.Product;

namespace BEAPI.Services.IServices
{
    public interface IProductService
    {
        Task Create(ProductCreateDto dto);
        Task<List<ProductDto>> GetAll();
        Task<ProductDto> GetById(string productId);
    }
}
