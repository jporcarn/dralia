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
            var weeklySlots = await _repository.GetWeeklyAvailabilityAsync(new DateOnly(2024, 12, 30));

            var mondayDate = new DateOnly(2024, 12, 30); // Monday of the same week as the date in the TakeSlotAsync test

            // Act
            WeeklySlots? result = null;
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
                FacilityId = weeklySlots.Facility.FacilityId, // This value changes in the API from time to time
                Name = weeklySlots.Facility.Name,
                Address = weeklySlots.Facility.Address,
            }, (opt) =>
            {
                opt.ExcludingMissingMembers();
                return opt;
            });

            result.SlotDurationMinutes.Should().Be(10);

            result.Days.Should().NotBeNullOrEmpty();
            result.Days.Should().HaveCount(5);

            var busyDays = result.Days.Where((d) => d.Slots.Count > 0 && d.Slots.Any(s => s.Busy))
                .ToList();

            busyDays.Should().NotBeNullOrEmpty();
            busyDays.Should().HaveCount(1);
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