using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Docplanner.Api.Mappings.Profiles;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;
using Docplanner.Test.Utilities;
using FluentAssertions;
using Xunit;

namespace Docplanner.ApiTests.Unit.Mappings.Profiless.Tests
{
    public class WeeklySlotsProfileTests
    {
        private readonly IFixture _fixture;
        private readonly IMapper _mapper;

        public WeeklySlotsProfileTests()
        {
            // Initialize AutoMapper with the relevant profiles
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FacilityProfile>();
                cfg.AddProfile<DailySlotsProfile>();
                cfg.AddProfile<SlotProfile>();
                cfg.AddProfile<WorkPeriodProfile>();

                cfg.AddProfile<WeeklySlotsProfile>();
            });

            _mapper = config.CreateMapper();

            // Initialize AutoFixture
            _fixture = new Fixture().Customize(new AutoMoqCustomization())
                .Customize(new DateOnlyCustomization()); // Prevent AutoFixture was unable to create an instance from System.DateOnly
        }

        [Fact]
        public void WeeklySlotsProfile_ConfigurationIsValid()
        {
            // Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void WeeklySlotsProfile_Should_Map_WeeklySlots_To_WeeklySlotsResponse()
        {
            // Arrange
            var domainObject = _fixture.Create<WeeklySlots>();

            // Act
            var result = _mapper.Map<WeeklySlotsResponse>(domainObject);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(domainObject, options => options.ExcludingMissingMembers());
        }
    }
}