// Copyright (c) 2016, Andreas Grimme

using System;

namespace hagen.plugin.office
{
    internal static class DateTimeExtensions
    {
        public static DateTime NextFullHour(this DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0).AddHours(1);
        }

        public static DateTime EndOfWeek(this DateTime t)
        {
            return t.Next(DayOfWeek.Friday);
        }

        public static DateTime Tomorrow(this DateTime t)
        {
            return t.Date.AddDays(1);
        }

        public static DateTime Next(this DateTime t, DayOfWeek w)
        {
            int d = (int)w - (int)t.DayOfWeek;
            if (t.DayOfWeek >= w)
            {
                d += 7;
            }
            return t.Date.AddDays(d);
        }

        public static DateTime Next(this DateTime t, int month, int day)
        {
            var n = new DateTime(t.Year, month, day);
            if (n < t)
            {
                n = n.AddYears(1);
            }
            return n;
        }

        public static DateTime Next(this DateTime t, int day)
        {
            var n = new DateTime(t.Year, t.Month, day);
            if (n < t)
            {
                n = n.AddMonths(1);
            }
            return n;
        }

        public static string GetHumanreadableRelativeTime(this DateTime t)
        {
            var r = t - DateTime.Now;
            if (r < TimeSpan.Zero)
            {
                return String.Format("Since {0:F0} minutes", -r.TotalMinutes);
            }
            else
            {
                return String.Format("In {0:F0} minutes", r.TotalMinutes);
            }
        }
    }
}
