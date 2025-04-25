using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Docplanner.Api.Mappings.Profiles;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;
using Docplanner.Test.Utilities;
using FluentAssertions;
using Xunit;

namespace Docplanner.Api.Mappings.Profiles.Tests
{
    public class DailySlotsProfileTests
    {
        private readonly IFixture _fixture;
        private readonly IMapper _mapper;

        public DailySlotsProfileTests()
        {
            // Initialize AutoMapper with the relevant profiles
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<WorkPeriodProfile>();
                cfg.AddProfile<SlotProfile>();

                cfg.AddProfile<DailySlotsProfile>();
            });

            _mapper = config.CreateMapper();

            // Initialize AutoFixture
            _fixture = new Fixture().Customize(new AutoMoqCustomization())
                .Customize(new DateOnlyCustomization()); // Prevent AutoFixture was unable to create an instance from System.DateOnly
        }

        [Fact]
        public void DailySlotsProfile_ConfigurationIsValid()
        {
            // Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void DailySlotsProfile_Should_Map_DailySlots_To_DailySlotsResponse()
        {
            // Arrange
            var domainObject = _fixture.Create<DailySlots>();

            // Act
            var result = _mapper.Map<DailySlotsResponse>(domainObject);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(domainObject, options => options.ExcludingMissingMembers());
        }
    }
}