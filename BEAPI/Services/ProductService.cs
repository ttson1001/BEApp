using AutoMapper;
using BEAPI.Controllers;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Product;
using BEAPI.Dtos.Value;
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

        public async Task Update(ProductCreateDto dto, string id)
        {
            var productId = GuidHelper.ParseOrThrow(id, "productId");

            var product = await _productRepo.Get()
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategoryValues)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantValues)
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new Exception("Product not found");

            var guidValueIds = dto.ProductVariants
                .SelectMany(v => v.ValueIds)
                .Select(valueId => GuidHelper.ParseOrThrow(valueId, nameof(valueId)))
                .ToList();

            var notFoundIds = await ValidationHelper.ValidateIdsExistAsync(_valueRepository, guidValueIds);

            if (notFoundIds.Any())
                throw new Exception($"ValueIds not found: {string.Join(", ", notFoundIds)}");

            product.Name = dto.Name;
            product.Brand = dto.Brand;
            product.Description = dto.Description;
            product.VideoPath = dto.VideoPath;
            product.Weight = dto.Weight;
            product.Height = dto.Height;
            product.Length = dto.Length;
            product.Width = dto.Width;
            product.ManufactureDate = dto.ManufactureDate;
            product.ExpirationDate = dto.ExpirationDate;

            product.ProductImages.Clear();
            foreach (var img in dto.ProductImages)
            {
                product.ProductImages.Add(new ProductImage { URL = img.URL });
            }

            product.ProductCategoryValues.Clear();
            foreach (var categoryId in dto.ValueCategoryIds)
            {
                product.ProductCategoryValues.Add(new ProductCategoryValue
                {
                    ValueId = GuidHelper.ParseOrThrow(categoryId, nameof(categoryId))
                });
            }

            product.ProductVariants.Clear();
            foreach (var variantDto in dto.ProductVariants)
            {
                var variant = new ProductVariant
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
                };

                product.ProductVariants.Add(variant);
            }

            _productRepo.Update(product);
            await _productRepo.SaveChangesAsync();
        }

        public async Task Create(ProductCreateDto dto)
        {
            var entity = _mapper.Map<Product>(dto);

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

            entity.ProductCategoryValues = dto.ValueCategoryIds.Select(categoryId => new ProductCategoryValue
            {
                ValueId = GuidHelper.ParseOrThrow(categoryId, nameof(categoryId))
            }).ToList();

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
                .Include(x => x.ProductCategoryValues)
                    .ThenInclude(x => x.Value)
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

        public async Task<PagedResult<ProductListDto>> SearchAsync(ProductSearchDto dto)
        {
            var query = _productRepo.Get()
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategoryValues)
                    .ThenInclude(pcv => pcv.Value)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Keyword))
                query = query.Where(p => p.Name.Contains(dto.Keyword) || p.Brand.Contains(dto.Keyword));

            if (dto.CategoryId.HasValue)
                query = query.Where(p => p.ProductCategoryValues.Any(c => c.ValueId == dto.CategoryId.Value));

            if (dto.MinPrice.HasValue)
                query = query.Where(p => p.ProductVariants
                    .OrderBy(v => v.CreationDate)
                    .Select(v => v.Price)
                    .FirstOrDefault() >= dto.MinPrice.Value);

            if (dto.MaxPrice.HasValue)
                query = query.Where(p => p.ProductVariants
                    .OrderBy(v => v.CreationDate)
                    .Select(v => v.Price)
                    .FirstOrDefault() <= dto.MaxPrice.Value);

            query = dto.SortBy.ToLower() switch
            {
                "name" => dto.SortDirection == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "brand" => dto.SortDirection == "asc" ? query.OrderBy(p => p.Brand) : query.OrderByDescending(p => p.Brand),
                "price" => dto.SortDirection == "asc"
                    ? query.OrderBy(p => p.ProductVariants.OrderBy(v => v.CreationDate).Select(v => v.Price).FirstOrDefault())
                    : query.OrderByDescending(p => p.ProductVariants.OrderBy(v => v.CreationDate).Select(v => v.Price).FirstOrDefault()),
                _ => dto.SortDirection == "asc" ? query.OrderBy(p => p.CreationDate) : query.OrderByDescending(p => p.CreationDate)
            };

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(p => new ProductListDto
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    Brand = p.Brand,
                    Price = p.ProductVariants
                        .OrderBy(v => v.CreationDate)
                        .Select(v => v.Price)
                        .FirstOrDefault(),
                    Description = p.Description,
                    ImageUrl = p.ProductImages
                        .OrderBy(img => img.CreationDate)
                        .Select(img => img.URL)
                        .FirstOrDefault(),
                    Categories = p.ProductCategoryValues
                        .Select(c => new ValueDto
                        {
                            Id = c.Value.Id.ToString(),
                            Code = c.Value.Code,
                            Label = c.Value.Label,
                            Description = c.Value.Description
                        }).ToList()
                })
                .ToListAsync();

            return new PagedResult<ProductListDto>
            {
                TotalItems = totalItems,
                Page = dto.Page,
                PageSize = dto.PageSize,
                Items = items
            };
        }

    }
}
