using AutoMapper;
using Docplanner.Api.Handlers;
using Docplanner.Api.Models;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Infrastructure.SlotService.Mappings.Profiles;
using Docplanner.Infrastructure.SlotService.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace Docplanner.ApiTests.Integration.SlotService.Repositories
{
    public class AvailabilityApiRepositoryTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IAvailabilityRepository _repository;

        public AvailabilityApiRepositoryTests(WebApplicationFactory<Program> factory)
        {
            #region Setup Using the DI Container

            // Create a service scope to resolve dependencies
            var scope = factory.Services.CreateScope();
            _repository = scope.ServiceProvider.GetRequiredService<IAvailabilityRepository>();

            #endregion Setup Using the DI Container

            #region Setup Using the Configuration

            // Load configuration from appsettings.json and appsettings.Development.json
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string baseUrlString = _configuration.GetValue<string>("AvailabilityApi:BaseUrl") ?? "http://localhost/api";

            // Create the custom handler
            var credentialsConfig = _configuration.GetSection("AvailabilityApi:Credentials");
            var basicAuthHandler = new BasicAuthHandler(
                credentialsConfig["Username"] ?? string.Empty,
                credentialsConfig["Password"] ?? string.Empty
            );

            // Manually chain the handlers
            var handler = new HttpClientHandler();
            basicAuthHandler.InnerHandler = handler;

            _httpClient = new HttpClient(basicAuthHandler)
            {
                BaseAddress = new Uri(baseUrlString),
            };

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AvailabilityProfile>();
            }).CreateMapper();

            #endregion Setup Using the Configuration
        }

        [Fact()]
        public async Task GetWeeklyAvailabilityAsync_Should_Return_Sucessful_StatusCode()
        {
            // Arrange
            var today = DateTime.Today;

            // Calculate the Monday of the current week
            var mondayOfTodaysWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            // If today is Sunday, adjust to the previous Monday
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                mondayOfTodaysWeek = today.AddDays(-6);
            }

            var mondayDateOnly = new DateOnly(mondayOfTodaysWeek.Year, mondayOfTodaysWeek.Month, mondayOfTodaysWeek.Day);
            var repository = new AvailabilityApiRepository(_httpClient, _mapper);

            // Act
            WeeklySlots? result = null;
            HttpResponseMessage? response = null;
            try
            {
                result = await repository.GetWeeklyAvailabilityAsync(mondayDateOnly);
                response = new HttpResponseMessage(HttpStatusCode.OK); // success
            }
            catch (HttpRequestException)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError); // failure
            }

            // Assert
            Assert.Contains((int)response.StatusCode, new[] { 200, 201, 202, 204 }); // Assert successful status codes

            result.Should().NotBeNull();
        }

        [Fact()]
        public async Task GetWeeklyAvailabilityAsync_Using_DI_Container_Should_Succeed()
        {
            // Arrange
            var mondayDate = new DateOnly(2025, 04, 14); // date in the past with fixed and known results.

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
            result.Facility.Should().BeEquivalentTo(new Facility
            {
                FacilityId = Guid.Parse("a6882e6c-cf3d-40a4-93d8-4584894fc539"),
                Name = "Las Palmeras",
                Address = "Plaza de la independencia 36, 38006 Santa Cruz de Tenerife"
            });

            result.SlotDurationMinutes.Should().Be(10);

            result.Days.Should().NotBeNullOrEmpty();
            result.Days.Should().HaveCount(5);

            var busyDays = result.Days.Where((d) => d.Slots.Count > 0 && d.Slots.Any(s => s.Busy))
                .ToList();

            busyDays.Should().NotBeNullOrEmpty();
            busyDays.Should().HaveCount(2);
        }
    }
}