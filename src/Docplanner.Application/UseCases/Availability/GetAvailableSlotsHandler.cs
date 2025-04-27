using AutoMapper;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Docplanner.Application.UseCases.Availability
{

    /// <summary>
    /// Handler for getting available slots for a given week and year.
    /// </summary>
    /// <remarks>This layer is responsible for orchestrating use cases and implementing business logic. Complex mapping logic that involves business rules, strategies, or configurations</remarks>
    public class GetAvailableSlotsHandler : IGetAvailableSlotsHandler
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly ILogger<GetAvailableSlotsHandler> _logger;
        private readonly IMapper _mapper;

        public GetAvailableSlotsHandler(
            IAvailabilityRepository availabilityRepository,
            ILogger<GetAvailableSlotsHandler> logger,
            IMapper mapper
            )
        {
            _availabilityRepository = availabilityRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<WeeklySlots> GetWeeklySlotsAsync(int year, int week)
        {
            DateOnly mondayDateOnly = Utilities.DateUtilities.GetMondayOfGivenYearAndWeek(year, week);

            var weeklyAvailabilityDto = await this._availabilityRepository.GetWeeklyAvailabilityAsync(mondayDateOnly);

            var weeklySlots = _mapper.Map<WeeklySlots>(weeklyAvailabilityDto, opts =>
            {
                opts.Items["mondayDateOnly"] = mondayDateOnly;
            });

            if (weeklySlots == null)
            {
                throw new KeyNotFoundException("No slots found for the given year and week.");
            }

            // Empty slots are included to fill the gaps in the schedule.
            var slots = weeklySlots.Days.SelectMany((d) => d.Slots);

            if (slots.Any())
            {
                // Get the minimum time of the week
                var minWeeklySlotTime = slots
                    .Select((d) =>
                    {
                        return new DateTime(mondayDateOnly.Year, mondayDateOnly.Month, mondayDateOnly.Day, d.Start.Hour, d.Start.Minute, d.Start.Second, DateTimeKind.Utc);
                    })
                    .Min();

                // Get the minimum time of the week
                var maxWeeklySlotTime = weeklySlots.Days.SelectMany((d) => d.Slots)
                    .Select((d) =>
                    {
                        return new DateTime(mondayDateOnly.Year, mondayDateOnly.Month, mondayDateOnly.Day, d.Start.Hour, d.Start.Minute, d.Start.Second, DateTimeKind.Utc);
                    })
                    .Max();

                FillGapsWithEmptySlots(weeklySlots, minWeeklySlotTime, maxWeeklySlotTime);
            }

            return weeklySlots;
        }

        private static void FillGapsWithEmptySlots(WeeklySlots weeklySlots, DateTime minWeeklySlotTime, DateTime maxWeeklySlotTime)
        {
            // Fill in empty slots

            weeklySlots.Days.ForEach((dailySlots) =>
            {

                // Top down
                var currentDayMinTime = dailySlots.Slots.Count > 0 ? dailySlots.Slots.Min((s) => s.Start) : minWeeklySlotTime;
                var bottomMinTime = new DateTime(currentDayMinTime.Year, currentDayMinTime.Month, currentDayMinTime.Day, minWeeklySlotTime.Hour, minWeeklySlotTime.Minute, minWeeklySlotTime.Second, DateTimeKind.Utc);

                var gapMinutes = currentDayMinTime.Subtract(bottomMinTime).TotalMinutes;
                var emptySlotsNumber = (int)(gapMinutes / weeklySlots.SlotDurationMinutes);

                for (int i = 0; i < emptySlotsNumber; i++)
                {
                    var emptySlotStart = bottomMinTime.AddMinutes(i * weeklySlots.SlotDurationMinutes);

                    dailySlots.Slots.Add(new Slot
                    {
                        Busy = false,
                        Empty = true,
                        End = emptySlotStart.AddMinutes(weeklySlots.SlotDurationMinutes),
                        Start = emptySlotStart,
                    });
                }


                // Bottom Up
                var currentDayMaxTime = dailySlots.Slots.Count > 0 ? dailySlots.Slots.Max((s) => s.Start) : maxWeeklySlotTime;
                var bottomMaxTime = new DateTime(currentDayMaxTime.Year, currentDayMaxTime.Month, currentDayMaxTime.Day, maxWeeklySlotTime.Hour, maxWeeklySlotTime.Minute, maxWeeklySlotTime.Second, DateTimeKind.Utc);

                gapMinutes = bottomMaxTime.Subtract(currentDayMaxTime).TotalMinutes;
                emptySlotsNumber = (int)(gapMinutes / weeklySlots.SlotDurationMinutes);

                for (int i = 0; i < emptySlotsNumber; i++)
                {
                    var emptySlotStart = currentDayMaxTime.AddMinutes((i + 1) * weeklySlots.SlotDurationMinutes);

                    dailySlots.Slots.Add(new Slot
                    {
                        Busy = false,
                        Empty = true,
                        End = emptySlotStart.AddMinutes(weeklySlots.SlotDurationMinutes),
                        Start = emptySlotStart,
                    });
                }


                dailySlots.Slots = dailySlots.Slots
                    .OrderBy((s) => s.Start)
                    .ThenBy((s) => s.End)
                    .ToList();

            });
        }
    }
}