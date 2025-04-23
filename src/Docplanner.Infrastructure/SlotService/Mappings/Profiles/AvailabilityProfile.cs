using AutoMapper;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Mappings.Resolvers;
using Docplanner.Infrastructure.SlotService.Models;

namespace Docplanner.Infrastructure.SlotService.Mappings.Profiles
{
    public class AvailabilityProfile : Profile
    {
        public AvailabilityProfile()
        {
            CreateMap<WeeklyAvailabilityDto, WeeklySlots>(MemberList.Destination)
                .ForMember(d => d.Days, opt =>
                {
                    opt.MapFrom<DailySlotsResolver>();
                });

            CreateMap<FacilityDto, Facility>(MemberList.Destination);

            CreateMap<WorkPeriodDto, WorkPeriod>(MemberList.Destination);
        }
    }
}