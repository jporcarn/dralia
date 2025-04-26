using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;

namespace Docplanner.Api.Mappings.Profiles
{
    public class WorkPeriodProfile : Profile
    {
        public WorkPeriodProfile()
        {
            CreateMap<WorkPeriod, WorkPeriodResponse>(MemberList.Destination);
        }
    }
}