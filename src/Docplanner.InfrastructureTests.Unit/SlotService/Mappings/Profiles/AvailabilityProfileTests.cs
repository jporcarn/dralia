using AutoMapper;
using Docplanner.Infrastructure.SlotService.Mappings.Profiles;
using Xunit;

namespace Docplanner.InfrastructureTests.Unit.SlotService.Mappings.Profiles
{
    public class AvailabilityProfileTests
    {
        private readonly IMapper _mapper;

        public AvailabilityProfileTests()
        {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AvailabilityProfile>();
            }).CreateMapper();
        }

        [Fact()]
        public void AvailabilityProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}