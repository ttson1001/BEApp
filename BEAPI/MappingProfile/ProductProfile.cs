using AutoMapper;
using BEAPI.Dtos.Product;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<ProductCreateDto, Product>()
                .ForMember(d => d.ProductTypeId, o => o.MapFrom(s => Guid.Parse(s.ProductTypeId)));

            CreateMap<Product, ProductDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.ProductTypeId, o => o.MapFrom(s => s.ProductTypeId.ToString()));

            CreateMap<ProductImageCreateDto, ProductImage>();

            CreateMap<ProductVariantCreateDto, ProductVariant>();

            CreateMap<ProductImage, ProductImageDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()));

            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.ProductVariantValues, o => o.MapFrom(s => s.ProductVariantValues));

            CreateMap<ProductVariantValue, ProductVariantValueDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.ValueId, o => o.MapFrom(s => s.ValueId.ToString()))
                .ForMember(d => d.ValueCode, o => o.MapFrom(s => s.Value.Code))
                .ForMember(d => d.ValueLabel, o => o.MapFrom(s => s.Value.Label));
        }
    }
}
