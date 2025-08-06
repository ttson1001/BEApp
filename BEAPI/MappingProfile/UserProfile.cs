using AutoMapper;
using BEAPI.Constants;
using BEAPI.Dtos.Auth;
using BEAPI.Dtos.Elder;
using BEAPI.Dtos.User;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using Org.BouncyCastle.Crypto.Generators;

namespace BEAPI.MappingProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.PasswordHash,
                    opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.RoleId,
                    opt => opt.MapFrom(src => Guid.Parse(UserContanst.UserRoleId)));
            CreateMap<User, UserListDto>();

            CreateMap<ElderRegisterDto, User>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.BirthDate,
                    opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.RoleId,
                    opt => opt.MapFrom(src => Guid.Parse(UserContanst.ElderRoleId)));

            CreateMap<User, ElderDto>()
              .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate.HasValue ? src.BirthDate.Value.DateTime : DateTime.MinValue))
              .ForMember(dest => dest.SpendLimit, opt => opt.MapFrom(src => src.Spendlimit ?? 0))
              .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.HasValue ? (int)src.Gender.Value : 0))
               .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id.ToString()))
              .ForMember(dest => dest.IsDelete, opt => opt.MapFrom(src => src.IsDeleted));

            CreateMap<ElderUpdateDto, User>()
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => new DateTimeOffset(src.BirthDate)))
                .ForMember(dest => dest.Spendlimit, opt => opt.MapFrom(src => src.SpendLimit))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => (Gender)src.Gender))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

        }
    }
}
