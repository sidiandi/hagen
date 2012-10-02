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

        public TimeInterval()
        {
        }

        public bool Contains(DateTime x)
        {
            return Begin <= x && x < End;
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

        public static TimeInterval Last(TimeSpan t)
        {
            var n = DateTime.Now;
            return new TimeInterval(n - t, n);
        }

        public static TimeInterval LastDays(int d)
        {
            var t = new TimeSpan(d, 0, 0, 0);
            var n = DateTime.Now.Date.AddDays(1);
            return new TimeInterval(n - t, n);
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
