// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

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
