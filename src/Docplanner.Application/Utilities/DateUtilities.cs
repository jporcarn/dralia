namespace Docplanner.Application.Utilities
{
    internal class DateUtilities
    {
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