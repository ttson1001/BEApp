using AutoMapper;
using BEAPI.Dtos.Cart;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<CartItemReplaceAllDto, CartItem>()
                .ForMember(d => d.ProductVariantId, o => o.MapFrom(s => Guid.Parse(s.ProductVariantId)))
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity));
        }
    }
}
