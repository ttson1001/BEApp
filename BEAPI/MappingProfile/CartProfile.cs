using AutoMapper;
using BEAPI.Dtos.Cart;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<CartItemCreateDto, CartItem>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => Guid.Parse(s.ProductId)))
                .ForMember(d => d.ElderId, o => o.MapFrom(s => Guid.Parse(s.ElderId)))
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity));
        }

    }
}
