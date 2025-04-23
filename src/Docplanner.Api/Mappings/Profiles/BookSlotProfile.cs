using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;

namespace Docplanner.Api.Mappings.Profiles
{
    public class BookSlotProfile : Profile
    {
        public BookSlotProfile()
        {
            CreateMap<BookSlotRequest, BookSlot>(MemberList.Destination)
                .ForMember(dest => dest.Patient, opt => opt.MapFrom(src => src.Patient));
        }
    }
}
