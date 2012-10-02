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
using Sidi.Persistence;

namespace hagen
{
    public static class InputEx
    {
        public static Input First(this Collection<Input> inputs)
        {
            return inputs.Select("1 order by begin limit 1").First();
        }

        public static Input Last(this Collection<Input> inputs)
        {
            return inputs.Select("1 order by begin desc limit 1").First();
        }

        public static TimeSpan Active(this IEnumerable<Input> data)
        {
            return data.Aggregate(TimeSpan.Zero, (a, x) =>
            {
                return a.Add(x.End - x.Begin);
            });
        }

        public static WorkInterval AtOffice(this IEnumerable<Input> data)
        {
            if (data.Any(x => !x.TerminalServerSession))
            {
                var w = new WorkInterval();
                w.TimeInterval.Begin = data.First(x => !x.TerminalServerSession).Begin;
                w.TimeInterval.End = data.Last(x => !x.TerminalServerSession).End;
                if (w.TimeInterval.Duration > Contract.Current.MaxWorkTimePerDay)
                {
                    w.TimeInterval.End = w.TimeInterval.Begin + Contract.Current.MaxWorkTimePerDay;
                }
                w.Place = Place.Office;
                return w;
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<WorkInterval> WorkIntervals(this IEnumerable<Input> data)
        {
            WorkInterval atOffice = null;
            WorkInterval extraOffice = null;
            DateTime leave = DateTime.MinValue;

            if (data.Any(x => !x.TerminalServerSession))
            {
                atOffice = new WorkInterval();
                atOffice.TimeInterval.Begin = data.First(x => !x.TerminalServerSession).Begin;
                leave = data.Last(x => !x.TerminalServerSession).End;
                atOffice.TimeInterval.End = leave;
                if (atOffice.TimeInterval.Duration > Contract.Current.MaxWorkTimePerDay)
                {
                    atOffice.TimeInterval.End = atOffice.TimeInterval.Begin + Contract.Current.MaxWorkTimePerDay;

                    extraOffice = new WorkInterval();
                    extraOffice.TimeInterval.Begin = atOffice.TimeInterval.End;
                    extraOffice.TimeInterval.End = leave;
                    extraOffice.Place = Place.OverHr;
                }
                atOffice.Place = Place.Office;
            }

            IEnumerable<Input> atHome;
            if (atOffice != null)
            {
                yield return atOffice;
                if (extraOffice != null)
                {
                    yield return extraOffice;
                }

                atHome = data.Where(x => x.End < atOffice.TimeInterval.Begin || x.Begin > leave);
            }
            else
            {
                atHome = data;
            }

            WorkInterval w = null;
            foreach (var i in atHome)
            {
                if (w != null)
                {
                    if (w.TimeInterval.End + Contract.Current.MaxHomeOfficeIdleTime >= i.Begin)
                    {
                        w.TimeInterval.End = i.End;
                    }
                    else
                    {
                        yield return w;
                        w = null;
                    }
                }

                if (w == null)
                {
                    w = new WorkInterval();
                    w.Place = Place.Home;
                    w.TimeInterval.Begin = i.Begin;
                    w.TimeInterval.End = i.End;
                }
            }
            if (w != null)
            {
                yield return w;
            }
        }
    }
}
