using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docplanner.Test.Utilities
{
    public class DateUtilities
    {
        public static int GetRandomWeekNumber(int year)
        {
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            var lastDay = new DateTime(year, 12, 31);
            var maxWeeks = calendar.GetWeekOfYear(lastDay, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            return Random.Shared.Next(1, maxWeeks + 1);
        }
    }
}