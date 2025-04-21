using Docplanner.Api.Models;
using Docplanner.Application.UseCases.Availability;
using Microsoft.AspNetCore.Mvc;

namespace Docplanner.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SlotController : ControllerBase
    {
        private readonly IGetAvailableSlotsHandler _getAvailableSlotsHandler;
        private readonly ILogger<SlotController> _logger;

        public SlotController(
            IGetAvailableSlotsHandler getAvailableSlotsHandler,
            ILogger<SlotController> logger
            )
        {
            _getAvailableSlotsHandler = getAvailableSlotsHandler;
            _logger = logger;
        }

        [HttpGet(Name = "GetWeeklySlots")]
        [ProducesResponseType(typeof(WeeklySlotsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WeeklySlotsResponse>> GetWeeklySlots([FromQuery] int year, [FromQuery] int week)
        {
            this._logger.LogInformation("GetWeeklySlots called with week: {week}", week);

            if (week is < 1 or > 53)
            {
                return BadRequest("Week number must be between 1 and 53.");
            }

            try
            {
                var result = await this._getAvailableSlotsHandler.GetWeeklySlotsAsync(year, week);
                if (result == null)
                {
                    return NotFound("No slots found for the given week.");
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
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