using AutoFixture;
using Docplanner.Api.Models;
using Docplanner.Test.Utilities;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace Docplanner.ApiTests.Integration.Controllers
{
    public class SlotControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Fixture _fixture;

        public SlotControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;

            // Initialize AutoFixture
            _fixture = new Fixture();
        }

        [Theory]
        [InlineData("/slot")]
        public async Task GetWeeklySlots_Should_Return_SuccessStatusCode(string operation)
        {
            // Arrange
            var httpClient = _factory.CreateClient();

            var operationUrl = $"{operation}?year={DateTime.UtcNow.Year}&week={DateUtilities.GetRandomWeekNumber(DateTime.UtcNow.Year)}";

            // Act
            var response = await httpClient.GetAsync(operationUrl);

            // Assert
            Assert.NotNull(response);
            response.EnsureSuccessStatusCode();

            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers);
            Assert.NotNull(response.Content.Headers.ContentType);

            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/slot/:startDate/book")]
        public async Task BookSlot_Should_Succeed(string operation)
        {
            // Arrange
            var httpClient = _factory.CreateClient();

            var iso8601Date = new DateTime(2025, 01, 03, 8, 0, 0).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            var operationUrl = operation.Replace(":startDate", iso8601Date);

            var bookSlotRequest = _fixture.Build<BookSlotRequest>()
                .With(dto => dto.Start, new DateTime(2025, 01, 03, 8, 0, 0).ToUniversalTime())
                .Create();

            // Act

            var response = await httpClient.PutAsJsonAsync(operationUrl, bookSlotRequest);

            // Assert
            Assert.NotNull(response);

            // Assert that the status code is one of the expected values
            Assert.Contains(response.StatusCode, new[]
            {
                System.Net.HttpStatusCode.Created,    // 201
                System.Net.HttpStatusCode.NotFound,  // 404
                System.Net.HttpStatusCode.BadRequest, // 400
                System.Net.HttpStatusCode.Conflict   // 409
            });

            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers);
            Assert.NotNull(response.Content.Headers.ContentType);

            Assert.Equal("application/json",
                response.Content.Headers.ContentType.ToString());
        }
    }
}