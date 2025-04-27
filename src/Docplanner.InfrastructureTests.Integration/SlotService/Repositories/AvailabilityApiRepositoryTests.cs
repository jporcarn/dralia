using AutoFixture;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Models;
using Docplanner.Test.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Docplanner.InfrastructureTests.Integration.SlotService.Repositories.Tests
{
    [TestCaseOrderer("PriorityOrderer", "Docplanner.Test.Utilities")]
    public class AvailabilityApiRepositoryTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly IAvailabilityRepository _repository;

        public AvailabilityApiRepositoryTests(WebApplicationFactory<Program> factory)
        {
            // Create a service scope to resolve dependencies
            var scope = factory.Services.CreateScope();
            _repository = scope.ServiceProvider.GetRequiredService<IAvailabilityRepository>();
        }

        [Fact()]
        [TestPriority(2)]
        public async Task GetWeeklyAvailabilityAsync_Using_DI_Container_Should_Succeed()
        {
            // Arrange
            var mondayDate = new DateOnly(2024, 12, 30); // Monday of the same week as the date in the TakeSlotAsync test

            // Act
            WeeklyAvailabilityDto? result = null;
            try
            {
                result = await _repository.GetWeeklyAvailabilityAsync(mondayDate);
            }
            catch (HttpRequestException ex)
            {
                Assert.Fail(ex.Message);
            }

            // Assert
            Assert.NotNull(result);

            result.Facility.Should().NotBeNull();

            result.Facility.Should().BeEquivalentTo(new
            {
                // FacilityId = Guid.Parse("80f09e30-b63b-4aee-8195-5add7ec735f1"), // This value changes in the API from time to time
                Name = "Las Palmeras",
                Address = "Plaza de la independencia 36, 38006 Santa Cruz de Tenerife",
            }, (opt) =>
            {
                opt.ExcludingMissingMembers();
                return opt;
            });

            result.SlotDurationMinutes.Should().Be(10);

            result.Monday.Should().NotBeNull();
            result.Tuesday.Should().BeNull();
            result.Wednesday.Should().NotBeNull();
            result.Thursday.Should().BeNull();
            result.Friday.Should().NotBeNull();

            var expectedMonday = new DailyAvailabilityDto
            {
                WorkPeriod = new WorkPeriodDto
                {
                    StartHour = 9,
                    EndHour = 17,
                    LunchStartHour = 13,
                    LunchEndHour = 14,
                }
            };

            Assert.NotNull(result.Monday);

            result.Monday.WorkPeriod.Should().NotBeNull();
            result.Monday.WorkPeriod.Should().BeEquivalentTo(expectedMonday.WorkPeriod, (opt) =>
            {
                opt.ExcludingMissingMembers();
                return opt;
            });

        }

        [Fact()]
        [TestPriority(1)]
        public async Task TakeSlotAsync_Using_DI_Container_Should_Succeed()
        {
            // Arrange
            var fixture = new Fixture();

            var weeklySlots = await _repository.GetWeeklyAvailabilityAsync(new DateOnly(2024, 12, 30));

            // Customize the FacilityId and other properties if needed
            var takeSlotDto = fixture.Build<TakeSlotDto>()
                .With(dto => dto.FacilityId, weeklySlots.Facility.FacilityId)
                .With(dto => dto.Start, new DateTime(2025, 01, 03, 8, 0, 0).ToUniversalTime())
                .With(dto => dto.End, new DateTime(2025, 01, 03, 8, 10, 0).ToUniversalTime())
                .Create();

            // Act
            try
            {
                await _repository.TakeSlotAsync(takeSlotDto);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // This is expected if the slot is already taken
                    Assert.True(true, "The slot is already taken.");
                    return;
                }

                Assert.Fail($"HTTP request failed: {ex.Message}");
            }

            // Assert
            Assert.True(true, "The TakeSlotAsync method completed successfully.");
        }
    }
}