using AutoMapper;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Models;
using Microsoft.Extensions.Logging;

namespace Docplanner.Infrastructure.SlotService.Mappings.Resolvers
{
    /// <summary>
    /// Resolves the daily slots for a given week based on the weekly availability data.
    /// </summary>
    /// <remarks>The Docplanner.Infrastructure project should only handle simple data transformation to domain models, leaving any business-specific mapping to the application layer.
    /// By Design: CET (Central European Time) as Default Time Zone for the Availability API
    /// Since the third-party availability API does not provide time zone information, I have assumed a default time zone for the data it returns.
    /// </remarks>
    public class DailySlotsResolver : IValueResolver<WeeklyAvailabilityDto, WeeklySlots, List<DailySlots>>
    {
        // Define a default time zone (e.g., CET) for interpreting the work periods returned by the availability API.
        private static readonly TimeZoneInfo DefaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        private readonly ILogger<DailySlotsResolver> _logger;

        public DailySlotsResolver(ILogger<DailySlotsResolver> logger)
        {
            _logger = logger;
        }

        public List<DailySlots> Resolve(WeeklyAvailabilityDto source, WeeklySlots destination, List<DailySlots> destMember, ResolutionContext context)
        {
            List<DailySlots> dailySlotsList = new();

            if (context == null)
            {
                throw new InvalidOperationException("Data Processing Error.", new ArgumentNullException(nameof(context), "Context cannot be null."));
            }

            var mondayDate = context.Items?["mondayDateOnly"] as DateOnly?;

            if (mondayDate == null)
            {
                throw new InvalidOperationException("Data Processing Error.", new ArgumentNullException(nameof(mondayDate), "Monday date cannot be null."));
            }

            var dailyAvailabilityList = new List<DailyAvailabilityDto?> { source.Monday, source.Tuesday, source.Wednesday, source.Thursday, source.Friday };

            for (int i = 0; i < dailyAvailabilityList.Count; i++)
            {
                DailyAvailabilityDto? dailyAvailability = dailyAvailabilityList[i];

                DateOnly weekDay = mondayDate.Value.AddDays(i);

                var dailySlots = new DailySlots
                {
                    Date = weekDay,
                    DayOfWeek = weekDay.DayOfWeek.ToString(),
                };

                if (dailyAvailability != null)
                {
                    dailySlots.Slots = GetSlots(weekDay, dailyAvailability, source.SlotDurationMinutes);
                    dailySlots.WorkPeriod = context.Mapper.Map<WorkPeriod>(dailyAvailability.WorkPeriod);
                }
                dailySlotsList.Add(dailySlots);
            }

            return dailySlotsList;
        }

        private List<Slot> GetSlots(DateOnly weekDay, DailyAvailabilityDto dailyAvailabilityDto, int slotDurationMinutes)
        {
            if (dailyAvailabilityDto.WorkPeriod == null)
            {
                return new List<Slot>();
            }

            var workPeriod = dailyAvailabilityDto.WorkPeriod;
            var busySlots = dailyAvailabilityDto.BusySlots ?? new List<BusySlotDto>();

            // Define the start and end times of the work period

            // Define the start and end times of the work period in CET
            DateTime startTimeUnspecified = new(weekDay.Year, weekDay.Month, weekDay.Day, workPeriod.StartHour, 0, 0, DateTimeKind.Unspecified);
            _logger.LogInformation($"Unspecified Start Time: {startTimeUnspecified} (assumed CET)");
            _logger.LogInformation($"Unspecified Start Time: {startTimeUnspecified.ToString("o")} (assumed CET)");

            DateTime endTimeUnspecified = new(weekDay.Year, weekDay.Month, weekDay.Day, workPeriod.EndHour, 0, 0, DateTimeKind.Unspecified);

            var startTimeUtc = TimeZoneInfo.ConvertTimeToUtc(startTimeUnspecified, DefaultTimeZone);
            _logger.LogInformation($"UTC Start Time: {startTimeUtc} (converted to UTC)");
            _logger.LogInformation($"UTC Start Time: {startTimeUtc.ToString("o")} (converted to UTC)");

            var endTimeUtc = TimeZoneInfo.ConvertTimeToUtc(endTimeUnspecified, DefaultTimeZone);

            // Exclude lunch break (convert lunch times to UTC)
            var lunchStartTimeCET = new DateTime(weekDay.Year, weekDay.Month, weekDay.Day, workPeriod.LunchStartHour, 0, 0, DateTimeKind.Unspecified);
            var lunchEndTimeCET = new DateTime(weekDay.Year, weekDay.Month, weekDay.Day, workPeriod.LunchEndHour, 0, 0, DateTimeKind.Unspecified);

            var lunchStartTimeUtc = TimeZoneInfo.ConvertTimeToUtc(lunchStartTimeCET, DefaultTimeZone);
            var lunchEndTimeUtc = TimeZoneInfo.ConvertTimeToUtc(lunchEndTimeCET, DefaultTimeZone);

            var slots = new List<Slot>();
            var slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);

            // Generate slots
            for (var currentTimeUtc = startTimeUtc; currentTimeUtc < endTimeUtc; currentTimeUtc += slotDuration)
            {
                bool isLunchBreak = currentTimeUtc >= lunchStartTimeUtc && currentTimeUtc < lunchEndTimeUtc;

                var slotEndTimeUtc = currentTimeUtc + slotDuration;

                // Determine if the slot is busy
                var isBusy = busySlots.Any(busySlot =>
                    currentTimeUtc < busySlot.End && slotEndTimeUtc > busySlot.Start);

                slots.Add(new Slot
                {
                    Busy = isBusy,
                    Empty = isLunchBreak,
                    End = slotEndTimeUtc,
                    Start = currentTimeUtc,
                });
            }

            return slots;
        }
    }
}