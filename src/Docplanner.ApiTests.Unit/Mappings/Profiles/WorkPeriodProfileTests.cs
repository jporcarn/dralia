using AutoFixture;
using AutoMapper;
using Docplanner.Api.Mappings.Profiles;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;
using FluentAssertions;
using Xunit;

namespace Docplanner.ApiTests.Unit.Mappings.Profiless.Tests
{
    public class WorkPeriodProfileTests
    {
        private readonly Fixture _fixture;
        private readonly IMapper _mapper;

        public WorkPeriodProfileTests()
        {
            // Initialize AutoMapper with the relevant profiles
            var config = new MapperConfiguration(cfg =>
            {
                // cfg.AddProfile<>();
                cfg.AddProfile<WorkPeriodProfile>();
            });

            _mapper = config.CreateMapper();

            // Initialize AutoFixture
            _fixture = new Fixture();
        }

        [Fact]
        public void WorkPeriodProfile_ConfigurationIsValid()
        {
            // Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void WorkPeriodProfile_Should_Map_WorkPeriod_To_WorkPeriodResponse()
        {
            // Arrange
            var domainObject = _fixture.Create<WorkPeriod>();

            // Act
            var result = _mapper.Map<WorkPeriodResponse>(domainObject);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(domainObject, options => options.ExcludingMissingMembers());
        }
    }
}