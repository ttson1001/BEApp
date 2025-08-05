using AutoMapper;
using BEAPI.Constants;
using BEAPI.Dtos.Auth;
using BEAPI.Dtos.User;
using BEAPI.Entities;
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
        }
    }
}
