using AutoFixture;
using AutoMapper;
using Docplanner.Api.Mappings.Profiles;
using Docplanner.Api.Models;
using Docplanner.Domain.Models;
using FluentAssertions;
using Xunit;

namespace Docplanner.ApiTests.Unit.Mappings.Profiless.Tests
{
    public class BookSlotProfileTests
    {
        private readonly Fixture _fixture;
        private readonly IMapper _mapper;

        public BookSlotProfileTests()
        {
            // Initialize AutoMapper with the relevant profiles
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PatientProfile>();
                cfg.AddProfile<BookSlotProfile>();
            });

            _mapper = config.CreateMapper();

            // Initialize AutoFixture
            _fixture = new Fixture();
        }

        [Fact]
        public void BookSlotProfile_ConfigurationIsValid()
        {
            // Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void BookSlotProfile_Should_Map_BookSlotRequest_To_BookSlot()
        {
            // Arrange
            var bookSlotRequest = _fixture.Create<BookSlotRequest>();

            // Act
            var result = _mapper.Map<BookSlot>(bookSlotRequest);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(bookSlotRequest, options => options.ExcludingMissingMembers());
        }
    }
}