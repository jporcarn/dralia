using AutoMapper;
using Docplanner.Domain.Models;
using Docplanner.Infrastructure.SlotService.Models;

namespace Docplanner.Infrastructure.SlotService.Mappings.Resolvers
{
    /// <summary>
    /// Resolves the daily slots for a given week based on the weekly availability data.
    /// </summary>
    /// <remarks>The Docplanner.Infrastructure project should only handle simple data transformation to domain models, leaving any business-specific mapping to the application layer.</remarks>
    public class DailySlotsResolver : IValueResolver<WeeklyAvailabilityDto, WeeklySlots, List<DailySlots>>
    {
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
            var startTime = new DateTime(weekDay.Year, weekDay.Month, weekDay.Day, workPeriod.StartHour, 0, 0, DateTimeKind.Local);
            var endTime = new DateTime(weekDay.Year, weekDay.Month, weekDay.Day, workPeriod.EndHour, 0, 0, DateTimeKind.Local);

            // Exclude lunch break
            var lunchStartTime = new DateTime(weekDay.Year, weekDay.Month, weekDay.Day, workPeriod.LunchStartHour, 0, 0, DateTimeKind.Local);
            var lunchEndTime = new DateTime(weekDay.Year, weekDay.Month, weekDay.Day, workPeriod.LunchEndHour, 0, 0, DateTimeKind.Local);

            var slots = new List<Slot>();
            var slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);

            // Generate slots
            for (var currentTime = startTime; currentTime < endTime; currentTime += slotDuration)
            {
                bool isLunchBreak = currentTime >= lunchStartTime && currentTime < lunchEndTime;

                var slotEndTime = currentTime + slotDuration;

                // Determine if the slot is busy
                var isBusy = busySlots.Any(busySlot =>
                    currentTime < busySlot.End && slotEndTime > busySlot.Start);

                slots.Add(new Slot
                {
                    Busy = isBusy,
                    Empty = isLunchBreak,
                    End = slotEndTime.ToUniversalTime(),
                    Start = currentTime.ToUniversalTime(),
                });
            }

            return slots;
        }
    }
}