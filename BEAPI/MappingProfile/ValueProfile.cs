using AutoMapper;
using BEAPI.Dtos.Category;
using BEAPI.Dtos.ListOfValue;
using BEAPI.Dtos.Value;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class ValueProfile : Profile
    {
        public ValueProfile()
        {
            CreateMap<ListOfValueCreateDto, ListOfValue>()
                .ForMember(dest => dest.Label,
                    opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.Note,
                    opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Guid.NewGuid()));

            CreateMap<ListOfValueUpdateDto, ListOfValue>()
               .ForMember(dest => dest.Label,
                   opt => opt.MapFrom(src => src.Label))
               .ForMember(dest => dest.Note,
                   opt => opt.MapFrom(src => src.Note))
               .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type))
               .ForMember(dest => dest.Id,
                   opt => opt.MapFrom(src => src.Id));

            CreateMap<ValueCreateDto, Value>()
                .ForMember(dest => dest.ListOfValueId, opt => opt.Ignore())
                .ForMember(dest => dest.ChildListOfValueId, opt => opt.Ignore());

            CreateMap<ValueUpdateDto, Value>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.ChildListOfValueId, opt => opt.Ignore())
                .ForMember(dest => dest.ListOfValueId, opt => opt.Ignore());

            CreateMap<Value, ValueDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ListOfValueId, opt => opt.MapFrom(src => src.ListOfValueId.ToString()));
            
            CreateMap<ListOfValue, ListOfValueDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values));

            CreateMap<Value, CategoryValueDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ChildrenId, opt => opt.MapFrom(src => src.ChildListOfValue.Id.ToString() ?? null))
                .ForMember(dest => dest.ChildrentLabel, opt => opt.MapFrom(src => src.ChildListOfValue.Label.ToString() ?? null));

        }
    }
}
