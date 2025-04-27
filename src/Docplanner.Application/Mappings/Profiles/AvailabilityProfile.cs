using AutoMapper;
using Docplanner.Application.Mappings.Resolvers;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Models;

namespace Docplanner.Application.Mappings.Profiles
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