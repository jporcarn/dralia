using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;

namespace Docplanner.Api.Mappings.Profiles
{
    public class SlotProfile : Profile
    {
        public SlotProfile()
        {
            CreateMap<Slot, SlotResponse>(MemberList.Destination);
        }
    }
}