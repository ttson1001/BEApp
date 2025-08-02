using AutoMapper;
using BEAPI.Dtos.Product;
using BEAPI.Entities;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Value> _valueRepository;
        private readonly IMapper _mapper;
        
        public ProductService (IRepository<Product> productRepo, IMapper mapper, IRepository<Value> valueReposiroty)
        {
            _productRepo = productRepo;
            _mapper = mapper;
            _valueRepository = valueReposiroty;
        }
        public async Task Create(ProductCreateDto dto)
        {
            var entity = _mapper.Map<Product>(dto);
            entity.ProductTypeId = GuidHelper.ParseOrThrow(dto.ProductTypeId, nameof(dto.ProductTypeId));

            var guidValueIds = dto.ProductVariants
                .SelectMany(v => v.ValueIds)
                .Select(id => GuidHelper.ParseOrThrow(id, nameof(id)))
                .ToList();

            var existingIds = await _valueRepository.Get()
                .Where(v => guidValueIds.Contains(v.Id))
                .Select(v => v.Id)
                .ToListAsync();

            var notFoundIds = guidValueIds.Except(existingIds).ToList();

            if (notFoundIds.Any())
                throw new Exception($"ValueIds not found: {string.Join(", ", notFoundIds)}");

            entity.ProductImages = dto.ProductImages
                .Select(img => new ProductImage { URL = img.URL })
                .ToList();

            entity.ProductVariants = dto.ProductVariants
                .Select(variantDto => new ProductVariant
                {
                    Price = variantDto.Price,
                    Discount = variantDto.Discount,
                    Stock = variantDto.Stock,
                    IsActive = variantDto.IsActive,
                    ProductVariantValues = variantDto.ValueIds
                        .Select(valueId => new ProductVariantValue
                        {
                            ValueId = GuidHelper.ParseOrThrow(valueId, nameof(valueId))
                        }).ToList()
                }).ToList();

            await _productRepo.AddAsync(entity);
            await _productRepo.SaveChangesAsync();
        }

        public async Task<List<ProductDto>> GetAll()
        {
            var entities = await _productRepo.Get()
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantValues)
                        .ThenInclude(vv => vv.Value)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(entities);
        }

        public async Task<ProductDto> GetById(string productId)
        {
            var guidId = GuidHelper.ParseOrThrow(productId, nameof(productId));

            var entity = await _productRepo.Get()
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantValues)
                        .ThenInclude(vv => vv.Value)
                .FirstOrDefaultAsync(x => x.Id == guidId)
                ?? throw new Exception("Product not found");

            return _mapper.Map<ProductDto>(entity);
        }

    }
}
