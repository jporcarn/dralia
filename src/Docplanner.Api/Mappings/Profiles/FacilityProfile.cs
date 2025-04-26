using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;

namespace Docplanner.Api.Mappings.Profiles
{
    public class FacilityProfile : Profile
    {
        public FacilityProfile()
        {
            CreateMap<Facility, FacilityResponse>(MemberList.Destination);
        }
    }
}
