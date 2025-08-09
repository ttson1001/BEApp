using AutoMapper;
using BEAPI.Dtos.Report;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class ReportProfile : Profile
    {
        public ReportProfile() {
            CreateMap<Report, ReportDto>().ReverseMap();
            CreateMap<ReportCreateDto, Report>();
            CreateMap<ReportUpdateDto, Report>();

        }
    }
}
