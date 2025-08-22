using AutoMapper;
using BEAPI.Dtos.Order;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class OrderProfile: Profile
    {
        public OrderProfile() {
            CreateMap<Order, OrderDto>()
               .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
               .ForMember(dest => dest.ElderName, opt => opt.MapFrom(src => src.Elder.FullName))
               .ForMember(dest => dest.ShippingFee, opt => opt.MapFrom(src => src.ShippingFee))
               .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.OrderStatus.ToString()))
               .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));

            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest => dest.Style, opt => opt.MapFrom(src =>
                    string.Join(", ",
                        src.ProductVariant.ProductVariantValues.Select(v => v.Value.Label)
                    )
                ))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                    src.ProductVariant.ProductImages.Select(img => img.URL).ToList()
                ));

        }
    }
}
