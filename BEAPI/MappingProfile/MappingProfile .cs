using AutoMapper;
using BEAPI.Dtos.AppsetiingDto;
using BEAPI.Entities;

namespace BEAPI.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AppDbSettings, AppDbSettingsDto>().ReverseMap();
            CreateMap<CreateAppDbSettingsDto, AppDbSettings>();
            CreateMap<UpdateAppDbSettingsDto, AppDbSettings>();
        }
    }
}
