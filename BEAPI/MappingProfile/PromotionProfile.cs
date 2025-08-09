using AutoMapper;
using BEAPI.Dtos.Promotion;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class PromotionProfile : Profile
    {
        public PromotionProfile() {
            CreateMap<Promotion, PromotionDto>().ReverseMap();

            CreateMap<PromotionCreateDto, Promotion>();

            CreateMap<PromotionUpdateDto, Promotion>()
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
