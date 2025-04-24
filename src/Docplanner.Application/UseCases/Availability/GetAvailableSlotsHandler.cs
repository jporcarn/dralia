using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Docplanner.Application.UseCases.Availability
{

    public class GetAvailableSlotsHandler : IGetAvailableSlotsHandler
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly ILogger<GetAvailableSlotsHandler> _logger;

        public GetAvailableSlotsHandler(
            IAvailabilityRepository availabilityRepository,
            ILogger<GetAvailableSlotsHandler> logger
            )
        {
            this._availabilityRepository = availabilityRepository;
            this._logger = logger;
        }

        public async Task<WeeklySlots> GetWeeklySlotsAsync(int year, int week)
        {
            DateOnly mondayDateOnly = Utilities.DateUtilities.GetMondayOfGivenYearAndWeek(year, week);

            var slots = await this._availabilityRepository.GetWeeklyAvailabilityAsync(mondayDateOnly);
            if (slots == null)
            {
                throw new KeyNotFoundException("No slots found for the given year and week.");
            }

            return slots;
        }
    }
}