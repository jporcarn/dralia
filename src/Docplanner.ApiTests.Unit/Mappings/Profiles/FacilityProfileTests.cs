using AutoFixture;
using AutoMapper;
using Docplanner.Api.Mappings.Profiles;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;
using FluentAssertions;
using Xunit;

namespace Docplanner.ApiTests.Unit.Mappings.Profiless.Tests
{
    public class FacilityProfileTests
    {
        private readonly Fixture _fixture;
        private readonly IMapper _mapper;

        public FacilityProfileTests()
        {
            // Initialize AutoMapper with the relevant profiles
            var config = new MapperConfiguration(cfg =>
            {
                // cfg.AddProfile<>();

                cfg.AddProfile<FacilityProfile>();
            });

            _mapper = config.CreateMapper();

            // Initialize AutoFixture
            _fixture = new Fixture();
        }

        [Fact]
        public void FacilityProfile_ConfigurationIsValid()
        {
            // Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void FacilityProfile_Should_Map_Facility_To_FacilityResponse()
        {
            // Arrange
            var domainObject = _fixture.Create<Facility>();

            // Act
            var result = _mapper.Map<FacilityResponse>(domainObject);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(domainObject, options => options.ExcludingMissingMembers());
        }
    }
}