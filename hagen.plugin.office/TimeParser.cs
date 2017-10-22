using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sprache;
using System.Globalization;

using TimeGen = System.Func<hagen.plugin.office.TimeParser, System.DateTime>;

namespace hagen.plugin.office
{
    internal enum Unit
    {
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year
    };

    public class TimeParser
    {
        static Parser<string> InWord = Sprache.Parse.String("in").Token().Text();
        static Parser<string> UntilWord = Sprache.Parse.String("until").Token().Text();

        static Parser<double> Scalar =
            from numberString in Sprache.Parse.DecimalInvariant
            select Double.Parse(numberString);

        static Parser<int> Integer =
            from numberString in Sprache.Parse.Number
            select Int32.Parse(numberString);

        static Parser<TimeGen> CalendarWeek =
            from cwKeyWord in Keyword("cw")
            from weekOfYear in Integer
            select new TimeGen(p => DateFromWeek(p.referenceTime, weekOfYear));

        static Parser<char> Dot = Sprache.Parse.Char('.');

        static Parser<TimeGen> DateLiteral =
            from day in Integer
            from delim1 in Dot
            from month in Integer
            from delim2 in Dot
            from year in Integer
            select new TimeGen(_ => new DateTime(year, month, day));

        static Parser<TimeGen> DateLiteralMonth =
            from day in Integer
            from delim1 in Dot
            from month in Integer
            from delim2 in Dot
            select new TimeGen(_ => _.referenceTime.Next(month, day));

        static Parser<TimeGen> DateLiteralDay =
            from day in Integer
            from delim1 in Dot
            select new TimeGen(_ => _.referenceTime.Next(day));

        static Parser<string> Keyword(string w)
        {
            return Sprache.Parse.IgnoreCase(w).Token().Text();
        }

        static E ParseEnum<E>(string s) where E : IConvertible
        {
            int bestMatchScore = 0;
            int bestValue = -1;
            foreach (int e in Enum.GetValues(typeof(E)))
            {
                var score = GetEqualLength(Enum.GetName(typeof(E), (object)e), s);
                if (score > bestMatchScore)
                {
                    bestMatchScore = score;
                    bestValue = e;
                }
            }
            return (E) Enum.ToObject(typeof(E), bestValue);
        }

        static Parser<E> EnumParser<E>() where E : IConvertible
        {
            return from word in Sprache.Parse.Identifier(Sprache.Parse.Letter, Sprache.Parse.Letter).Token().Text()
            select ParseEnum<E>(word);
        }

        static Parser<TimeGen> Tomorrow =
            from t in Keyword("Tomorrow")
            select new TimeGen(x => x.referenceTime.Tomorrow());

        static Parser<TimeGen> Today =
            from t in Keyword("today")
            select new TimeGen(x => x.referenceTime.Date);

        static Parser<TimeGen> EndOfWeek =
            from t in Keyword("end of week").Or(Keyword("eow"))
            select new TimeGen(x => x.referenceTime.EndOfWeek());

        static Parser<TimeGen> Weekday =
            from dayOfWeek in EnumParser<DayOfWeek>()
            select new TimeGen(x => x.referenceTime.Next(dayOfWeek));

        static Parser<TimeGen> UntilExpression =
            from untilWord in UntilWord
            from date in CalendarWeek
            .Or(DateLiteral)
            .Or(DateLiteralMonth)
            .Or(DateLiteralDay)
            .Or(Tomorrow)
            .Or(Today)
            .Or(EndOfWeek)
            .Or(Weekday)
            select date;

        static Parser<TimeGen> Fixed(Parser<DateTime> dateTimeParser)
        {
            return from d in dateTimeParser select new TimeGen(x => d);
        }

        static DateTime Add(DateTime t, double s, Unit u)
        {
            switch (u)
            {
                case Unit.Second:
                    return t.AddSeconds(s);
                case Unit.Minute:
                    return t.AddMinutes(s);
                case Unit.Hour:
                    return t.AddHours(s);
                case Unit.Day:
                    return t.AddDays(s);
                case Unit.Week:
                    return t.AddDays(s * 7.0);
                case Unit.Month:
                    return t.AddMonths((int)s);
                case Unit.Year:
                    return t.AddYears((int)s);
            }
            throw new ArgumentOutOfRangeException("u", u, "unknown unit");
        }

        static int GetEqualLength(string a, string b)
        {
            var e = Math.Min(a.Length, b.Length);
            int i = 0;
            for (; i < e; ++i)
            {
                if (char.ToLowerInvariant(a[i]) != char.ToLowerInvariant(b[i]))
                {
                    return i;
                }
            }
            return i;
        }

        internal static Unit? GuessUnit(string u)
        {
            var units = Enum.GetNames(typeof(Unit));
            string bestUnit = null;
            int maxEqualLength = 0;
            foreach (var i in units)
            {
                var equalLength = GetEqualLength(u, i);
                if (equalLength > maxEqualLength)
                {
                    bestUnit = i;
                    maxEqualLength = equalLength;
                }
            }
            if (bestUnit == null)
            {
                return null;
            }
            return (Unit) Enum.Parse(typeof(Unit), bestUnit);
        }

        static Parser<Unit> UnitIdentifier =
            from word in Sprache.Parse.Identifier(Sprache.Parse.Letter, Sprache.Parse.Letter).Token().Text()
            select GuessUnit(word).Value;

        static Parser<TimeGen> InExpression =
            from inWord in InWord
            from scalar in Scalar
            from unit in UnitIdentifier
            select new TimeGen(x => Add(x.referenceTime, scalar, unit));

        static Parser<TimeGen> TimeString = UntilExpression.Or(InExpression);

        static Parser<string> OtherText = Sprache.Parse.AnyChar.Many().Text();

        static Parser<Func<TimeParser, Subject>> SubjectExpression=
            (
                from dueDate in TimeString
                from subject in OtherText
                select new Func<TimeParser, Subject>(tp => new Subject { DueDate = dueDate(tp), Text = subject })
            ).Or
            (
                from subject in Sprache.Parse.AnyChar.Except(TimeString).Many().Text()
                from dueDate in TimeString
                select new Func<TimeParser, Subject>(tp => new Subject { DueDate = dueDate(tp), Text = subject })
            )
            .Or
            (
                from subject in OtherText
                select new Func<TimeParser, Subject>(tp => new Subject { DueDate = tp.referenceTime, Text = subject })
            )
            ;

        public TimeParser()
        : this(DateTime.Now)
        { 
        }

        public TimeParser(DateTime dateTime)
        {
            this.referenceTime = dateTime;
        }

        readonly DateTime referenceTime;

        internal DateTime Parse(string timeString)
        {
            return TimeString.Parse(timeString)(this);
        }

        public class Subject
        {
            public DateTime DueDate { get; set; }
            public string Text { get; set; }
        }

        internal Subject ParseSubject(string subject)
        {
            return SubjectExpression.Parse(subject)(this);
        }

        static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }

        static DateTime DateFromWeek(DateTime referenceDate, int weekOfYear)
        {
            var year = referenceDate.Year;
            var d = FirstDateOfWeekISO8601(year, weekOfYear);
            if (d < referenceDate)
            {
                d = d.AddYears(1);
            }
            return d;
        }
    }
}
