using BEAPI.Dtos.Common;
using BEAPI.Dtos.Product;

namespace BEAPI.Services.IServices
{
    public interface IProductService
    {
        Task Create(ProductCreateDto dto);
        Task<List<ProductDto>> GetAll();
        Task<ProductDto> GetById(string productId, string role);
        Task<PagedResult<ProductListDto>> SearchAsync(ProductSearchDto dto);
        Task<PagedResult<ProductListDto>> SearchProductActiveAsync(ProductSearchDto dto);
        Task DeActiveOrActiveProduct(Guid productId);
        Task Update(ProductCreateDto dto, string id);
        Task<ProductWithTypeDto> GetWithStylesById(string productId);
    }
}
