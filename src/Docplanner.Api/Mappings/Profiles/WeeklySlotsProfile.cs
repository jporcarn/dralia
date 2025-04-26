using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;

namespace Docplanner.Api.Mappings.Profiles
{
    public class WeeklySlotsProfile : Profile
    {
        public WeeklySlotsProfile()
        {
            CreateMap<WeeklySlots, WeeklySlotsResponse>(MemberList.Destination);
        }
    }
}