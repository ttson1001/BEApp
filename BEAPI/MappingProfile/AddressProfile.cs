using AutoMapper;
using BEAPI.Dtos.Addreess;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class AddressProfile: Profile
    {
        public AddressProfile() {

            CreateMap<CreateAddressDto, Address>();
            CreateMap<Address, AddressDto>();
        }
    }
}
