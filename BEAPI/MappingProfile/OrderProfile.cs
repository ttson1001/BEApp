using AutoMapper;
using BEAPI.Dtos.Order;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class OrderProfile: Profile
    {
        public OrderProfile() {
            CreateMap<OrderCreateDto, Order>()
                .ForMember(d => d.CustomerId, o => o.MapFrom(s => Guid.Parse(s.CustomerId)))
                .ForMember(d => d.OrderDetails, o => o.MapFrom(s => s.OrderDetails));

            CreateMap<OrderDetailCreateDto, OrderDetail>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => Guid.Parse(s.ProductId)))
                .ForMember(d => d.ElderId, o => o.MapFrom(s => Guid.Parse(s.ElderId)));

            //CreateMap<Order, OrderDto>()
            //    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            //    .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.CustomerId.ToString()))
            //    .ForMember(d => d.OrderStatus, o => o.MapFrom(s => s.OrderStatus.ToString()));

            //CreateMap<OrderDetail, OrderDetailDto>()
            //    .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            //    .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId.ToString()));

        }
    }
}
