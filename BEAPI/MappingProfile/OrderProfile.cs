using AutoMapper;
using BEAPI.Dtos.Order;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class OrderProfile: Profile
    {
        public OrderProfile() {
            CreateMap<Order, OrderDto>()
                 .ForMember(dest => dest.ElderName, opt => opt.MapFrom(src => src.Elder != null ? src.Elder.FullName : string.Empty));

            CreateMap<OrderDetail, OrderDetailDto>();

        }
    }
}
