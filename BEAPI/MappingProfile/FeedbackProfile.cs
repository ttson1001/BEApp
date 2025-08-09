using AutoMapper;
using BEAPI.Dtos.Feedback;
using BEAPI.Entities;

namespace BEAPI.MappingProfile
{
    public class FeedbackProfile: Profile
    {
        public FeedbackProfile() {

            CreateMap<Feedback, FeedbackDto>();

            CreateMap<FeedbackCreateDto, Feedback>()
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<FeedbackUpdateDto, Feedback>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
