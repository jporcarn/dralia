using Docplanner.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Docplanner.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SlotController : ControllerBase
    {
        private readonly ILogger<SlotController> _logger;

        public SlotController(ILogger<SlotController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeeklySlots")]
        [ProducesResponseType(typeof(WeeklySlotsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<WeeklySlotsResponse>> GetWeeklySlots([FromQuery] int week)
        {
            this._logger.LogInformation("GetWeeklySlots called with week: {week}", week);

            if (week is < 1 or > 53)
            {
                return BadRequest("Week number must be between 1 and 53.");
            }

            var currentYear = DateTime.UtcNow.Year;
            var monday = FirstDateOfWeekISO8601(currentYear, week);

            var slotDuration = 10;
            var facility = new FacilityResponse
            {
                FacilityId = Guid.NewGuid(),
                Name = "Las Palmeras",
                Address = "Plaza de la independencia 36, 38006 Santa Cruz de Tenerife"
            };

            var days = Enumerable.Range(0, 7).Select(offset =>
            {
                var date = monday.AddDays(offset);
                var isWorkingDay = date.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday;

                var slots = new List<SlotResponse>();
                var workPeriod = new WorkPeriodResponse
                {
                    StartHour = 9,
                    EndHour = 17,
                    LunchStartHour = 13,
                    LunchEndHour = 14
                };

                if (isWorkingDay)
                {
                    var startTime = new DateTime(date.Year, date.Month, date.Day, workPeriod.StartHour, 0, 0);
                    var endTime = new DateTime(date.Year, date.Month, date.Day, workPeriod.EndHour, 0, 0);

                    for (var time = startTime; time < endTime; time = time.AddMinutes(slotDuration))
                    {
                        if (time.Hour >= workPeriod.LunchStartHour && time.Hour < workPeriod.LunchEndHour)
                            continue;

                        var isBusy = time.Minute % 30 == 0;
                        slots.Add(new SlotResponse
                        {
                            Start = time.ToUniversalTime(),
                            End = time.AddMinutes(slotDuration).ToUniversalTime(),
                            Busy = isBusy
                        });
                    }
                }

                return new DailySlotsResponse
                {
                    DayOfWeek = date.DayOfWeek.ToString(),
                    Date = DateOnly.FromDateTime(date),
                    WorkPeriod = isWorkingDay ? workPeriod : new WorkPeriodResponse(),
                    Slots = slots
                };
            }).ToList();

            var response = new WeeklySlotsResponse
            {
                Facility = facility,
                SlotDurationMinutes = slotDuration,
                Days = days
            };

            return Ok(await Task.FromResult(response));
        }

        private static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;
            var firstThursday = jan1.AddDays(daysOffset);

            var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            var firstWeek = cal.GetWeekOfYear(firstThursday, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
                weekNum -= 1;

            return firstThursday.AddDays(weekNum * 7).AddDays(-3); // Go back to Monday
        }
    }
}