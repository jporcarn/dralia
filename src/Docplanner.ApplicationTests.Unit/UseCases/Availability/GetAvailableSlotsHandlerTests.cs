using AutoFixture;
using AutoFixture.AutoMoq;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Application.UseCases.Availability;
using Docplanner.Domain.Models;
using Docplanner.Test.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Docplanner.ApplicationTests.Unit.UseCases.Availability
{
    public partial class GetAvailableSlotsHandlerTests
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
            var result = Application.Utilities.DateUtilities.GetMondayOfGivenYearAndWeek(year, week);

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

        [Fact]
        public async Task GetWeeklySlotsAsync_Should_Return_Correctly_Filled_Slots()
        {
            // Arrange
            var mockRepository = new Mock<IAvailabilityRepository>();
            var mockLogger = new Mock<ILogger<GetAvailableSlotsHandler>>();

            var mondayDate = new DateOnly(2024, 12, 30);
            var weeklySlots = new WeeklySlots
            {
                Days = new List<DailySlots>
                {
                    new DailySlots
                    {
                        Date = mondayDate,
                        DayOfWeek = "Monday",
                        Slots = GetWeekDaySlots("Monday"),
                        WorkPeriod = new WorkPeriod { StartHour = 9, EndHour = 17, LunchStartHour = 13, LunchEndHour = 14 }
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(1),
                        DayOfWeek = "Tuesday",
                        Slots = GetWeekDaySlots("Tuesday"),
                        WorkPeriod = null
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(2),
                        DayOfWeek = "Wednesday",
                        Slots = GetWeekDaySlots("Wednesday"),
                        WorkPeriod = new WorkPeriod { StartHour = 9, EndHour = 17, LunchStartHour = 13, LunchEndHour = 14 }
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(3),
                        DayOfWeek = "Thursday",
                        Slots = GetWeekDaySlots("Thursday"),
                        WorkPeriod = null
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(4),
                        DayOfWeek = "Friday",
                        Slots = GetWeekDaySlots("Friday"),
                        WorkPeriod = new WorkPeriod { StartHour = 8, EndHour = 16, LunchStartHour = 13, LunchEndHour = 14 }
                    }
                },
                Facility = new Facility { FacilityId = Guid.NewGuid(), Name = "Test Facility", Address = "Test Address" },
                SlotDurationMinutes = 10
            };

            mockRepository
                .Setup(repo => repo.GetWeeklyAvailabilityAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync(weeklySlots);

            var handler = new GetAvailableSlotsHandler(mockRepository.Object, mockLogger.Object);

            // Act
            var result = await handler.GetWeeklySlotsAsync(2025, 1);

            // Assert
            Assert.NotNull(result);

            // 1. Tuesday and Thursday slots are empty
            Assert.Empty(result.Days.First(d => d.DayOfWeek == "Tuesday").Slots);
            Assert.Empty(result.Days.First(d => d.DayOfWeek == "Thursday").Slots);
            Assert.Empty(result.Days.First(d => d.DayOfWeek == "Wednesday").Slots);

            // 2. Monday, and Friday have 6 empty slots
            Assert.Equal(6, result.Days.First(d => d.DayOfWeek == "Monday").Slots.Count(s => s.Empty));
            Assert.Equal(6, result.Days.First(d => d.DayOfWeek == "Friday").Slots.Count(s => s.Empty));

            // 3. Monday empty slots are from 8:00 AM to 8:50 AM
            var mondayEmptySlots = result.Days.First(d => d.DayOfWeek == "Monday").Slots.Where(s => s.Empty).ToList();
            Assert.Equal(new DateTime(2024, 12, 30, 7, 0, 0, DateTimeKind.Utc), mondayEmptySlots[0].Start);
            Assert.Equal(new DateTime(2024, 12, 30, 7, 50, 0, DateTimeKind.Utc), mondayEmptySlots.Last().Start);

            // 4. Friday empty slots are from 4:00 PM to 4:50 PM
            var fridayEmptySlots = result.Days.First(d => d.DayOfWeek == "Friday").Slots.Where(s => s.Empty).ToList();
            Assert.Equal(new DateTime(2025, 1, 3, 14, 50, 0, DateTimeKind.Utc), fridayEmptySlots[0].Start);
            Assert.Equal(new DateTime(2025, 1, 3, 15, 40, 0, DateTimeKind.Utc), fridayEmptySlots.Last().Start);
        }

        private static List<Slot> GetWeekDaySlots(string weekDay)
        {
            switch (weekDay)
            {
                case "Monday":
                    return new List<Slot>
                        {
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 8, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 8, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 8, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 8, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 8, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 8, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 8, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 8, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 8, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 8, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 8, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 9, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 9, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 9, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 9, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 9, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 9, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 9, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 9, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 9, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 9, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 9, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 9, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 10, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 10, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 10, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 10, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 10, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 10, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 10, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 10, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 10, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 10, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 10, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 10, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 11, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 11, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 11, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 11, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 11, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 11, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 11, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 11, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 11, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 11, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 11, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 11, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 13, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 13, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 13, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 13, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 13, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 13, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 13, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 13, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 13, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 13, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 13, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 14, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 14, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 14, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 14, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 14, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 14, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 14, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 14, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 14, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 14, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 14, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 14, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 15, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 15, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 15, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 15, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 15, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 15, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 15, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 15, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 15, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 15, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 15, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 15, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 16, 0, 0, DateTimeKind.Utc) }
                        };

                case "Tuesday":
                case "Wednesday":
                case "Thursday":
                    return new List<Slot>();

                case "Friday":
                    return new List<Slot>
                        {
                            new Slot { Busy = true, Empty = false, Start = new DateTime(2025, 1, 3, 7, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 7, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 7, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 7, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 7, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 7, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 7, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 7, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 7, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 7, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 7, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 8, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 8, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 8, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 8, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 8, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 8, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 8, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 8, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 8, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 8, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 8, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 8, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 9, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 9, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 9, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 9, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 9, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 9, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 9, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 9, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 9, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 9, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 9, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 9, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 10, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 10, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 10, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 10, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 10, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 10, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 10, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 10, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 10, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 10, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 10, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 10, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 11, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 11, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 11, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 11, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 11, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 11, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 11, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 11, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 11, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 11, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 11, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 11, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 13, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 13, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 13, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 13, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 13, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 13, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 13, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 13, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 13, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 13, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 13, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 14, 0, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 14, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 14, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 14, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 14, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 14, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 14, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 14, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 14, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 14, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 14, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 14, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 15, 0, 0, DateTimeKind.Utc) }
                        };

                default:
                    return new List<Slot>();
            }
        }
    }
}