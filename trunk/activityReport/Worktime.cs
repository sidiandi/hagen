using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hagen;

namespace activityReport
{
    ///Calculate work time according to German regulations
    public class Worktime
    {
        public static TimeSpan Pause = TimeSpan.FromHours(0.75);
        public static TimeSpan MaxWorkTimeWithoutPause = TimeSpan.FromHours(6);
        public static TimeSpan MaxWorkTimePerDay = TimeSpan.FromHours(10);
        public static TimeSpan RegularDailyWorkTime = TimeSpan.FromHours(8);

        public static bool IsWorkDay(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    return false;
                default:
                    return true;
            }
        }

        public static TimeSpan GetOfficialWorkTime(TimeInterval time)
        {
            var d = time.Duration;
            if (d <= MaxWorkTimeWithoutPause)
            {
                return time.Duration;
            }

            if (d <= MaxWorkTimeWithoutPause + Pause)
            {
                return MaxWorkTimeWithoutPause;
            }

            d = d - Pause;

            if (d < MaxWorkTimePerDay)
            {
                return d;
            }

            return MaxWorkTimePerDay;
        }
    }
}
