using Docplanner.Api.Models;
using Docplanner.Infrastructure.SlotService.Models;
using FluentAssertions;
using Xunit;

namespace Docplanner.Infrastructure.SlotService.Mappings.Resolvers.Tests
{
    public class DailySlotsResolverTests
    {
        [Fact()]
        public void DailySlotsResolver_Throws_Exception_When_No_Context()
        {
            // Arrange
            WeeklyAvailabilityDto source = new();
            WeeklySlots destination = new();
            List<DailySlots> destMember = new();

            var resolver = new DailySlotsResolver();

            // Act & Assert
            FluentActions.Invoking(() => resolver.Resolve(source, destination, destMember, null!))
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Data Processing Error.*");
        }
    }
}