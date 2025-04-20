using Microsoft.AspNetCore.Mvc.Testing;
using System;
using Xunit;

namespace Docplanner.ApiTests.Integration.Controllers
{
    public class SlotControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SlotControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/slot")]
        public async Task GetWeeklySlots_Should_Return_SuccessStatusCode(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            var slotUrl = $"{url}?year={DateTime.UtcNow.Year}&week={GetRandomWeekNumber(DateTime.UtcNow.Year)}";

            // Act
            var response = await client.GetAsync(slotUrl);

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        private int GetRandomWeekNumber(int year)
        {
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            var lastDay = new DateTime(year, 12, 31);
            var maxWeeks = calendar.GetWeekOfYear(lastDay, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            return Random.Shared.Next(1, maxWeeks + 1);
        }
    }
}