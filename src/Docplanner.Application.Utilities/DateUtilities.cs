using System.Globalization;

namespace Docplanner.Application.Utilities
{
    public class DateUtilities
    {
        /// <summary>
        /// Monday date based on ISO week date standard (ISO-8601)
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="week">Week</param>
        /// <returns></returns>
        public static DateOnly GetMondayOfGivenYearAndWeek(int year, int week)
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

        /// <summary>
        ///  Week number based on ISO week date standard (ISO-8601)
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int GetWeekOfYear(DateTime start)
        {
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(start, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return weekNumber;
        }
    }
}