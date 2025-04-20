using AutoMapper;
using Docplanner.Api.Models;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Infrastructure.SlotService.Models;
using System.Net.Http.Json;

namespace Docplanner.Infrastructure.SlotService.Repositories
{
    public class AvailabilityApiRepository : IAvailabilityRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;

        public AvailabilityApiRepository(HttpClient httpClient, IMapper mapper)
        {
            this._httpClient = httpClient;
            this._mapper = mapper;
        }

        public async Task<WeeklySlots> GetWeeklyAvailabilityAsync(DateOnly mondayDate)
        {
            // Operation Path: "GetWeeklyAvailability/{yyyyMMdd}"

            var mondayQueryParam = mondayDate.ToString("yyyyMMdd");

            var urlBuilder = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(_httpClient.BaseAddress?.ToString()))
                urlBuilder.Append(_httpClient.BaseAddress.ToString());

            urlBuilder.Append("/GetWeeklyAvailability");
            urlBuilder.Append($"/{System.Uri.EscapeDataString(mondayQueryParam)}");

            string operationUrl = urlBuilder.ToString();

            // var json = await _httpClient.GetStringAsync(operationUrl);

            var response = await _httpClient.GetFromJsonAsync<WeeklyAvailabilityDto>(operationUrl);

            var domainModel = _mapper.Map<WeeklySlots>(response, opts =>
            {
                opts.Items["mondayDateOnly"] = mondayDate;
            });

            return domainModel;
        }
    }
}