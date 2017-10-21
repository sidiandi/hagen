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
    }
}
