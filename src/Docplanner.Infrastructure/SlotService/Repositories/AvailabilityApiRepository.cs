using AutoMapper;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Docplanner.Infrastructure.SlotService.Repositories
{
    public class AvailabilityApiRepository : IAvailabilityRepository, IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AvailabilityApiRepository> _logger;
        private readonly IMapper _mapper;

        public AvailabilityApiRepository(
            HttpClient httpClient,
            ILogger<AvailabilityApiRepository> logger,
            IMapper mapper)
        {
            this._httpClient = httpClient;
            this._logger = logger;
            this._mapper = mapper;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if the Availability API is reachable

                var today = DateTime.Today;

                // Calculate the Monday of the current week
                var mondayOfTodaysWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
                var mondayDateOnly = DateOnly.FromDateTime(mondayOfTodaysWeek);

                var response = await this.GetWeeklyAvailabilityAsync(mondayDateOnly);
                return HealthCheckResult.Healthy("Availability API is reachable.");
            }
            catch (HttpRequestException ex)
            {
                var additionalData = new Dictionary<string, object>
                {
                    { "StatusCode", ex.StatusCode?.ToString() ?? "Unknown" },
                    { "RequestUri", _httpClient.BaseAddress?.ToString() ?? "Unknown" }
                };

                switch (ex.StatusCode)
                {
                    case System.Net.HttpStatusCode.Unauthorized:
                        additionalData.Add("Suggestion", "Credentials used to call the Availability API might be wrong or expired.");
                        return HealthCheckResult.Unhealthy("Unauthorized access to Availability API.", ex, additionalData);

                    case System.Net.HttpStatusCode.NotFound:
                        additionalData.Add("Suggestion", "The URL of the Availability API might be wrong or temporary unabailable.");
                        return HealthCheckResult.Unhealthy("Availability API not found.", ex, additionalData);

                    default:
                        break;
                }

                return HealthCheckResult.Unhealthy(ex.Message, ex);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Availability API health check failed.", ex);
            }
        }

        public async Task<WeeklySlots> GetWeeklyAvailabilityAsync(DateOnly mondayDate)
        {
            // Operation path: "GetWeeklyAvailability/{yyyyMMdd}"

            // Define the API endpoint
            var endpoint = "/GetWeeklyAvailability";

            var mondayQueryParam = mondayDate.ToString("yyyyMMdd");

            var urlBuilder = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(_httpClient.BaseAddress?.ToString()))
                urlBuilder.Append(_httpClient.BaseAddress.ToString());

            urlBuilder.Append(endpoint);
            urlBuilder.Append($"/{System.Uri.EscapeDataString(mondayQueryParam)}");

            string operationUrl = urlBuilder.ToString();

            var response = await _httpClient.GetFromJsonAsync<WeeklyAvailabilityDto>(operationUrl);

            var domainModel = _mapper.Map<WeeklySlots>(response, opts =>
            {
                opts.Items["mondayDateOnly"] = mondayDate;
            });

            return domainModel;
        }

        public async Task TakeSlotAsync(TakeSlotDto takeSlotDto)
        {
            // Operation path: "TakeSlot"

            // Define the API endpoint
            var endpoint = "/TakeSlot";

            // Build the full URL
            var urlBuilder = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(_httpClient.BaseAddress?.ToString()))
                urlBuilder.Append(_httpClient.BaseAddress.ToString());

            urlBuilder.Append(endpoint);

            string operationUrl = urlBuilder.ToString();

            // Make the POST request

            // Serialize the DTO to JSON for logging
            var serializedBody = System.Text.Json.JsonSerializer.Serialize(takeSlotDto, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            this._logger.LogInformation($"serializedBody: {serializedBody}");

            // Create the HTTP content
            var content = new StringContent(serializedBody, System.Text.Encoding.UTF8, "application/json");

            // Make the POST request
            var response = await _httpClient.PostAsync(operationUrl, content);

            // Handle the response
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Failed to take slot. Status Code: {response.StatusCode}, Response: {errorContent}",
                    null,
                    response.StatusCode);
            }
        }
    }
}