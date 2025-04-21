using Docplanner.Api.Models;
using Docplanner.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Docplanner.Application.UseCases.Availability
{
    public class GetAvailableSlotsHandler : IGetAvailableSlotsHandler
    {
        private readonly ILogger<GetAvailableSlotsHandler> _logger;
        private readonly IAvailabilityRepository _availabilityRepository;

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
            DateOnly mondayDateOnly = GetMondayOfGivenYearAndWeek(year, week);

            var slots = await this._availabilityRepository.GetWeeklyAvailabilityAsync(mondayDateOnly);
            if (slots == null)
            {
                throw new KeyNotFoundException("No slots found for the given year and week.");
            }

            return slots;
        }

        /// <summary>
        /// ISO week date standard (ISO-8601)
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="week">Week</param>
        /// <returns></returns>
        internal static DateOnly GetMondayOfGivenYearAndWeek(int year, int week)
        {
            // Calculate the first day of the year
            var firstDayOfYear = new DateTime(year, 1, 1);

            // Calculate the first Monday of the year
            var firstMonday = firstDayOfYear.AddDays(DayOfWeek.Monday - firstDayOfYear.DayOfWeek);


            // Calculate the Monday of the given week
            var mondayOfWeek = firstMonday.AddDays((week - 1) * 7);

            // Convert to DateOnly
            var mondayDateOnly = new DateOnly(mondayOfWeek.Year, mondayOfWeek.Month, mondayOfWeek.Day);
            return mondayDateOnly;
        }
    }
}