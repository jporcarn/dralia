using AutoFixture;
using AutoFixture.AutoMoq;
using Docplanner.Api.Models;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Application.UseCases.Availability;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Docplanner.ApplicationTests.Unit.UseCases.Availability
{
    public class GetAvailableSlotsHandlerTests
    {
        private readonly IFixture _fixture;

        public GetAvailableSlotsHandlerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization())
                .Customize(new DateOnlyCustomization()); // Prevent AutoFixture was unable to create an instance from System.DateOnly
        }

        [Theory]
        [InlineData(2024, 1, "2024-01-01")] // 1st week of 2024
        [InlineData(2024, 2, "2024-01-08")]
        [InlineData(2024, 39, "2024-09-23")]
        [InlineData(2024, 52, "2024-12-23")] // Last week of 2025
        [InlineData(2025, 1, "2024-12-30")] // 1st week of 2025
        [InlineData(2025, 2, "2025-01-06")]
        [InlineData(2025, 17, "2025-04-21")]
        [InlineData(2025, 52, "2025-12-22")] // Last week of 2025
        [InlineData(2026, 1, "2025-12-29")] // 1st week of 2026
        [InlineData(2026, 2, "2026-01-05")]
        [InlineData(2026, 29, "2026-07-13")]
        [InlineData(2026, 52, "2026-12-21")]
        [InlineData(2026, 53, "2026-12-28")] // Last week of 2026
        public void GetMondayOfGivenYearAndWeek_Should_Return_Correct_Monday(int year, int week, string expectedMonday)
        {
            // Act
            var result = GetAvailableSlotsHandler.GetMondayOfGivenYearAndWeek(year, week);

            // Assert
            result.Should().Be(DateOnly.Parse(expectedMonday));
        }

        [Theory()]
        [InlineData(2024, 1)]
        [InlineData(2025, 1)]
        public async Task GetWeeklySlotsAsync_Should_Return_WeeklySlots(int year, int week)
        {
            // Arrange
            var mockRepository = _fixture.Freeze<Mock<IAvailabilityRepository>>();
            var mockLogger = _fixture.Freeze<Mock<ILogger<GetAvailableSlotsHandler>>>();

            // Set up the mock repository to return a valid WeeklySlots object
            var expectedWeeklySlots = _fixture.Create<WeeklySlots>();
            mockRepository
                .Setup(repo => repo.GetWeeklyAvailabilityAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync(expectedWeeklySlots);

            var handler = new GetAvailableSlotsHandler(mockRepository.Object, mockLogger.Object);

            // Act
            var result = await handler.GetWeeklySlotsAsync(year, week);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedWeeklySlots);
        }

        [Fact]
        public async Task GetWeeklySlotsAsync_Should_Throw_Exception_When_No_Slots_Found()
        {
            // Arrange
            var mockRepository = _fixture.Freeze<Mock<IAvailabilityRepository>>();
            var mockLogger = _fixture.Freeze<Mock<ILogger<GetAvailableSlotsHandler>>>();

            // Set up the mock repository to return null
            mockRepository
                .Setup(repo => repo.GetWeeklyAvailabilityAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync((WeeklySlots)null!);

            var handler = new GetAvailableSlotsHandler(mockRepository.Object, mockLogger.Object);

            // Act
            Func<Task> act = async () => await handler.GetWeeklySlotsAsync(2025, 1);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("No slots found for the given year and week.");
        }

        public class DateOnlyCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customize<DateOnly>(composer =>
                    composer.FromFactory(() =>
                    {
                        var randomDate = fixture.Create<DateTime>();
                        return new DateOnly(randomDate.Year, randomDate.Month, randomDate.Day);
                    }));
            }
        }
    }
}