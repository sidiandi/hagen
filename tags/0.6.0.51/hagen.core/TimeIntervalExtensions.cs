using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;

namespace hagen
{
    public static class TimeIntervalExtensions
    {
        public static TimeInterval LastDays(int days)
        {
            var today = DateTime.Now.Date;
            return new TimeInterval(today.AddDays(-days), today.AddDays(1));
        }

        public static bool Contains(this TimeInterval timeInterval, DateTime t)
        {
            return timeInterval.Begin <= t && t < timeInterval.End;
        }
    }
}
