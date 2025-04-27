using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Application.UseCases.Availability;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Models;
using Docplanner.Test.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Docplanner.ApplicationTests.Unit.UseCases.Availability.Tests
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
            var result = Application.Utilities.DateUtilities.GetMondayOfGivenYearAndWeek(year, week);

            // Assert
            result.Should().Be(DateOnly.Parse(expectedMonday));
        }

        [Fact]
        public async Task GetWeeklySlotsAsync_Should_Return_Correctly_Extend_Schedule_With_Empty_Slots()
        {
            // Arrange
            var mockRepository = new Mock<IAvailabilityRepository>();
            var mockLogger = new Mock<ILogger<GetAvailableSlotsHandler>>();
            var mockMapper = _fixture.Freeze<Mock<IMapper>>();

            var expectedWeeklyAvailabilityDto = GetWeeklyAvailabilityDto();

            mockRepository
                .Setup(repo => repo.GetWeeklyAvailabilityAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync(expectedWeeklyAvailabilityDto);

            // Set up the mock mapper to return a valid object
            var mondayDate = new DateOnly(2024, 12, 30);
            var weeklySlots = GetWeeklySlots(mondayDate);
            mockMapper
                .Setup(m => m.Map<WeeklySlots>(It.IsAny<WeeklyAvailabilityDto>(), It.IsAny<Action<IMappingOperationOptions<object, WeeklySlots>>>()))
                .Returns(weeklySlots);

            var handler = new GetAvailableSlotsHandler(mockRepository.Object, mockLogger.Object, mockMapper.Object);

            // Act
            var result = await handler.GetWeeklySlotsAsync(2025, 1);

            // Assert
            Assert.NotNull(result);

            // 1. Tuesday, Wednesday and Thursday slots are empty
            Assert.Empty(result.Days.First(d => d.DayOfWeek == "Tuesday").Slots);
            Assert.Empty(result.Days.First(d => d.DayOfWeek == "Thursday").Slots);
            Assert.Empty(result.Days.First(d => d.DayOfWeek == "Wednesday").Slots);

            // 2. Monday, and Friday have 6 empty slots
            Assert.Equal(6 + 6, result.Days.First(d => d.DayOfWeek == "Monday").Slots.Count(s => s.Empty)); // 6+6: 6 empty slots lunch break + 6 empty slots due to extending the schedule
            Assert.Equal(6 + 6, result.Days.First(d => d.DayOfWeek == "Friday").Slots.Count(s => s.Empty)); // 6+6: 6 empty slots lunch break + 6 empty slots due to extending the schedule

            // 3. Monday:
            var mondayEmptySlots = result.Days.First(d => d.DayOfWeek == "Monday").Slots.Where(s => s.Empty).ToList();

            // 3.1. Extended empty slots are from 8:00 AM to 8:50 AM
            Assert.Equal(new DateTime(2024, 12, 30, 7, 0, 0, DateTimeKind.Utc), mondayEmptySlots[0].Start);
            Assert.Equal(new DateTime(2024, 12, 30, 7, 50, 0, DateTimeKind.Utc), mondayEmptySlots[5].Start);

            // 3.2. Lunch break slots are from 1:00 PM to 1:50 PM
            Assert.Equal(new DateTime(2024, 12, 30, 12, 0, 0, DateTimeKind.Utc), mondayEmptySlots[6].Start);
            Assert.Equal(new DateTime(2024, 12, 30, 12, 50, 0, DateTimeKind.Utc), mondayEmptySlots[11].Start);

            // 4. Friday:
            var fridayEmptySlots = result.Days.First(d => d.DayOfWeek == "Friday").Slots.Where(s => s.Empty).ToList();

            // 4.1. Extended empty slots are from 4:00 PM to 4:50 PM
            Assert.Equal(new DateTime(2025, 1, 3, 15, 00, 0, DateTimeKind.Utc), fridayEmptySlots[6].Start);
            Assert.Equal(new DateTime(2025, 1, 3, 15, 50, 0, DateTimeKind.Utc), fridayEmptySlots[11].Start);

            // 4.2. Lunch break slots are from 1:00 PM to 1:50 PM
            Assert.Equal(new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc), fridayEmptySlots[0].Start);
            Assert.Equal(new DateTime(2025, 1, 3, 12, 50, 0, DateTimeKind.Utc), fridayEmptySlots[5].Start);

        }

        [Theory()]
        [InlineData(2025, 1)] // [Theory, InlineData] Only for ilustration purposes
        public async Task GetWeeklySlotsAsync_Should_Return_WeeklySlots_Including_EmptySlots(int year, int week)
        {
            // Arrange
            var mockRepository = _fixture.Freeze<Mock<IAvailabilityRepository>>();
            var mockLogger = _fixture.Freeze<Mock<ILogger<GetAvailableSlotsHandler>>>();
            var mockMapper = _fixture.Freeze<Mock<IMapper>>();

            // Set up the mock repository to return a valid object
            var expectedWeeklyAvailabilityDto = GetWeeklyAvailabilityDto();
            mockRepository
                .Setup(repo => repo.GetWeeklyAvailabilityAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync(expectedWeeklyAvailabilityDto);

            // Set up the mock mapper to return a valid object
            var mondayDate = new DateOnly(2024, 12, 30);
            var weeklySlots = GetWeeklySlots(mondayDate);
            mockMapper
                .Setup(m => m.Map<WeeklySlots>(It.IsAny<WeeklyAvailabilityDto>(), It.IsAny<Action<IMappingOperationOptions<object, WeeklySlots>>>()))
                .Returns(weeklySlots);

            var handler = new GetAvailableSlotsHandler(mockRepository.Object, mockLogger.Object, mockMapper.Object);

            // Act
            var result = await handler.GetWeeklySlotsAsync(year, week);

            // Assert

            var expectedWeeklySlots = GetWeeklySlotsWithEmptyGaps(mondayDate);

            result.Should().NotBeNull();


            result.Days[0].Should().BeEquivalentTo(expectedWeeklySlots.Days[0]);
            result.Days[1].Should().BeEquivalentTo(expectedWeeklySlots.Days[1]);
            result.Days[2].Should().BeEquivalentTo(expectedWeeklySlots.Days[2]);
            result.Days[3].Should().BeEquivalentTo(expectedWeeklySlots.Days[3]);
            result.Days[4].Should().BeEquivalentTo(expectedWeeklySlots.Days[4]);

            result.Should().BeEquivalentTo(expectedWeeklySlots);
        }

        [Fact]
        public async Task GetWeeklySlotsAsync_Should_Throw_Exception_When_No_Slots_Found()
        {
            // Arrange
            var mockRepository = _fixture.Freeze<Mock<IAvailabilityRepository>>();
            var mockLogger = _fixture.Freeze<Mock<ILogger<GetAvailableSlotsHandler>>>();
            var mockMapper = _fixture.Freeze<Mock<IMapper>>();

            // Set up the mock repository to return null
            mockRepository
                .Setup(repo => repo.GetWeeklyAvailabilityAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync(GetWeeklyAvailabilityDto());

            // Set up the mock mapper to return a valid object
            var mondayDate = new DateOnly(2024, 12, 30);
            var weeklySlots = GetWeeklySlots(mondayDate);
            mockMapper
                .Setup(m => m.Map<WeeklySlots>(It.IsAny<WeeklyAvailabilityDto>(), It.IsAny<Action<IMappingOperationOptions<object, WeeklySlots>>>()))
                .Returns((WeeklySlots)null!);

            var handler = new GetAvailableSlotsHandler(mockRepository.Object, mockLogger.Object, mockMapper.Object);

            // Act
            Func<Task> act = async () => await handler.GetWeeklySlotsAsync(2025, 1);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("No slots found for the given year and week.");
        }

        private static WeeklySlots GetWeeklySlotsWithEmptyGaps(DateOnly mondayDate)
        {
            return new WeeklySlots
            {
                Facility = new Facility
                {
                    FacilityId = Guid.Parse("80f09e30-b63b-4aee-8195-5add7ec735f1"),
                    Name = "Las Palmeras",
                    Address = "Plaza de la independencia 36, 38006 Santa Cruz de Tenerife"
                },
                SlotDurationMinutes = 10,
                Days = new List<DailySlots>
                {
                    new DailySlots
                    {
                        Date = mondayDate,
                        DayOfWeek = "Monday",
                        Slots = new List<Slot>
                        {
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 7, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 7, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 7, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 7, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 7, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 7, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 7, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 7, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 7, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 7, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 7, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 8, 0, 0, DateTimeKind.Utc) },

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

                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 13, 0, 0, DateTimeKind.Utc) },

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
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2024, 12, 30, 15, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 16, 0, 0, DateTimeKind.Utc) },
                        },
                        WorkPeriod = new WorkPeriod { StartHour = 9, EndHour = 17, LunchStartHour = 13, LunchEndHour = 14 }
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(1),
                        DayOfWeek = "Tuesday",
                        Slots = new List<Slot>(),
                        WorkPeriod = null
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(2),
                        DayOfWeek = "Wednesday",
                        Slots = new List<Slot>(),
                        WorkPeriod = null
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(3),
                        DayOfWeek = "Thursday",
                        Slots = new List<Slot>(),
                        WorkPeriod = null
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(4),
                        DayOfWeek = "Friday",
                        Slots = new List<Slot>
                        {
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 7, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 7, 10, 0, DateTimeKind.Utc) },
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

                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 13, 0, 0, DateTimeKind.Utc) },

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
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 14, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 15, 0, 0, DateTimeKind.Utc) },

                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 15, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3,  15, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 15, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 15, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 15, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 15, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 15, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 15, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 15, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 15, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 15, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 16, 0, 0, DateTimeKind.Utc) }
                        },
                        WorkPeriod = new WorkPeriod { StartHour = 8, EndHour = 16, LunchStartHour = 13, LunchEndHour = 14 }
                    }
                },
            };
        }

        private static WeeklySlots GetWeeklySlots(DateOnly mondayDate)
        {
            return new WeeklySlots
            {
                Facility = new Facility
                {
                    FacilityId = Guid.Parse("80f09e30-b63b-4aee-8195-5add7ec735f1"),
                    Name = "Las Palmeras",
                    Address = "Plaza de la independencia 36, 38006 Santa Cruz de Tenerife"
                },
                SlotDurationMinutes = 10,
                Days = new List<DailySlots>
                {
                    new DailySlots
                    {
                        Date = mondayDate,
                        DayOfWeek = "Monday",
                        WorkPeriod = new WorkPeriod
                        {
                            StartHour = 9,
                            EndHour = 17,
                            LunchStartHour = 13,
                            LunchEndHour = 14
                        },
                        Slots = new List<Slot>
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

                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 0, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 10, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 20, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 30, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 40, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 12, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2024, 12, 30, 12, 50, 0, DateTimeKind.Utc), End = new DateTime(2024, 12, 30, 13, 0, 0, DateTimeKind.Utc) },

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
                        }
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(1),
                        DayOfWeek = "Tuesday",
                        WorkPeriod = null,
                        Slots = new List<Slot>()
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(2),
                        DayOfWeek = "Wednesday",
                        WorkPeriod = null,
                        Slots = new List<Slot>()
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(3),
                        DayOfWeek = "Thursday",
                        WorkPeriod = null,
                        Slots = new List<Slot>()
                    },
                    new DailySlots
                    {
                        Date = mondayDate.AddDays(4),
                        DayOfWeek = "Friday",
                        WorkPeriod = new WorkPeriod
                        {
                            StartHour = 8,
                            EndHour = 16,
                            LunchStartHour = 13,
                            LunchEndHour = 14
                        },
                        Slots = new List<Slot>
                        {
                            new Slot { Busy = false, Empty = false, Start = new DateTime(2025, 1, 3, 7, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 7, 10, 0, DateTimeKind.Utc) },
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

                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 10, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 10, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 20, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 20, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 30, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 30, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 40, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 40, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 12, 50, 0, DateTimeKind.Utc) },
                            new Slot { Busy = false, Empty = true, Start = new DateTime(2025, 1, 3, 12, 50, 0, DateTimeKind.Utc), End = new DateTime(2025, 1, 3, 13, 0, 0, DateTimeKind.Utc) },

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
                        }
                    },
                }
            };
        }

        private static WeeklyAvailabilityDto GetWeeklyAvailabilityDto()
        {
            return new WeeklyAvailabilityDto
            {
                Facility = new FacilityDto
                {
                    FacilityId = Guid.Parse("80f09e30-b63b-4aee-8195-5add7ec735f1"),
                    Name = "Las Palmeras",
                    Address = "Plaza de la independencia 36, 38006 Santa Cruz de Tenerife"
                },
                SlotDurationMinutes = 10,
                Monday = new DailyAvailabilityDto
                {
                    WorkPeriod = new WorkPeriodDto
                    {
                        StartHour = 9,
                        EndHour = 17,
                        LunchStartHour = 13,
                        LunchEndHour = 14
                    },
                    BusySlots = new List<BusySlotDto>
                    {
                        new BusySlotDto
                        {
                            Start = DateTime.Parse("2024-12-30T08:20:00Z"),
                            End = DateTime.Parse("2024-12-30T08:30:00Z")
                        }
                    }
                },
                Friday = new DailyAvailabilityDto
                {
                    WorkPeriod = new WorkPeriodDto
                    {
                        StartHour = 8,
                        EndHour = 16,
                        LunchStartHour = 13,
                        LunchEndHour = 14
                    },
                    BusySlots = new List<BusySlotDto>
                    {
                        new BusySlotDto
                        {
                            Start = DateTime.Parse("2025-01-03T07:00:00Z"),
                            End = DateTime.Parse("2025-01-03T07:10:00Z")
                        }
                    }
                }
            };
        }
    }
}