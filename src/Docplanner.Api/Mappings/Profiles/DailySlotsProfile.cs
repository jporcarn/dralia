using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;

namespace Docplanner.Api.Mappings.Profiles
{
    public class DailySlotsProfile : Profile
    {
        public DailySlotsProfile()
        {
            CreateMap<DailySlots, DailySlotsResponse>(MemberList.Destination);
        }
    }
}
