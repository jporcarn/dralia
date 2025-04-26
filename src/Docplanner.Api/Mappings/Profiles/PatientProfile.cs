using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;

namespace Docplanner.Api.Mappings.Profiles
{
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            CreateMap<PatientRequest, Patient>(MemberList.Destination);
        }
    }
}