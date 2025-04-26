using AutoFixture;
using AutoMapper;
using Docplanner.Api.Mappings.Profiles;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;
using FluentAssertions;
using Xunit;

namespace Docplanner.ApiTests.Unit.Mappings.Profiless.Tests
{
    public class SlotProfileTests
    {
        private readonly Fixture _fixture;
        private readonly IMapper _mapper;

        public SlotProfileTests()
        {
            // Initialize AutoMapper with the relevant profiles
            var config = new MapperConfiguration(cfg =>
            {
                //cfg.AddProfile<>();
                cfg.AddProfile<SlotProfile>();
            });

            _mapper = config.CreateMapper();

            // Initialize AutoFixture
            _fixture = new Fixture();
        }

        [Fact]
        public void SlotProfile_ConfigurationIsValid()
        {
            // Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void SlotProfile_Should_Map_Slot_To_SlotResponse()
        {
            // Arrange
            var domainObject = _fixture.Create<Slot>();

            // Act
            var result = _mapper.Map<SlotResponse>(domainObject);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(domainObject, options => options.ExcludingMissingMembers());
        }
    }
}