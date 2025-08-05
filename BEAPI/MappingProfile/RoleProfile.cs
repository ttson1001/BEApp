using AutoMapper;
using BEAPI.Dtos.Role;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class RoleProfile: Profile
    {
        public RoleProfile() {
            CreateMap<Role, RoleDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

            CreateMap<RoleDto, Role>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        }
    }
}
