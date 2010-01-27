using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Util;

namespace hagen
{
    public class TimeInterval
    {
        public TimeInterval(DateTime begin, DateTime end)
        {
            Begin = begin;
            End = end;
        }

        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration
        { 
            get 
            { 
                return End - Begin; 
            } 
        }

        public override string ToString()
        {
            return "[{0:o}; {1:o}[".F(Begin, End);
        }

        public IEnumerable<TimeInterval> Days
        {
            get
            {
                for (var i = Begin.Date; i < End; i = i.AddDays(1))
                {
                    yield return new TimeInterval(i, i.AddDays(1));
                }
            }
        }

        public IEnumerable<TimeInterval> Months
        {
            get
            {
                for (var i = new DateTime(Begin.Year, Begin.Month, 1); i < End; i = i.AddMonths(1))
                {
                    yield return new TimeInterval(i, i.AddMonths(1));
                }
            }
        }

        [TestFixture]
        public class Test
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            [Test]
            public void Days()
            {
                var d = new TimeInterval(new DateTime(2009, 1, 20), new DateTime(2009, 1, 25, 2, 2, 1)).Days;
                Assert.AreEqual(6, d.Count());
            }

            [Test]
            public void Months()
            {
                var d = new TimeInterval(new DateTime(2009, 1, 20), new DateTime(2009, 1, 25, 2, 2, 1)).Months;
                Assert.AreEqual(1, d.Count());
                var f = d.First();
                Assert.AreEqual(1, f.Begin.Month);
                Assert.AreEqual(2, f.End.Month);
            }
        }
    }
}
