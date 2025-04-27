using AutoFixture;
using Docplanner.Application.UseCases.Availability;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Mappings.Resolvers;
using Docplanner.Infrastructure.SlotService.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Docplanner.InfrastructureTests.Unit.SlotService.Mappings.Resolvers.Tests
{
    public class DailySlotsResolverTests
    {
        private readonly IFixture _fixture;

        public DailySlotsResolverTests()
        {
            _fixture = new Fixture();
        }

        [Fact()]
        public void DailySlotsResolver_Throws_Exception_When_No_Context()
        {
            // Arrange
            WeeklyAvailabilityDto source = new();
            WeeklySlots destination = new();
            List<DailySlots> destMember = new();

            var mockLogger = _fixture.Freeze<Mock<ILogger<DailySlotsResolver>>>();

            var resolver = new DailySlotsResolver(mockLogger.Object);

            // Act & Assert
            FluentActions.Invoking(() => resolver.Resolve(source, destination, destMember, null!))
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Data Processing Error.*");
        }
    }
}