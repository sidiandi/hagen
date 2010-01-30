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
            WorkInterval atOffice = data.AtOffice();
        
            var atHome = data;
            if (atOffice != null)
            {
                yield return atOffice;
                atHome = data.Where(x => x.End < atOffice.TimeInterval.Begin || x.Begin > atOffice.TimeInterval.End);
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
