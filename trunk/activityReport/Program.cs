// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using Sidi.Util;
using Sidi.CommandLine;
using hagen;
using Sidi.Persistence;
using Sidi.IO;
using System.Data.Linq;
using System.Data.SQLite;
using System.Windows.Forms;
using ZedGraph;
using System.Drawing;
using System.IO;
using NUnit.Framework;

namespace activityReport
{
    public class Program
    {
        static void Main(string[] args)
        {
            Parser.Run(new Program(), args);
        }

        public Program()
        {
            SQLiteFunction.RegisterFunction(typeof(Duration));
            input = Hagen.Instance.Inputs;
            connection = (System.Data.SQLite.SQLiteConnection)input.Connection;
            dataContext = new DataContext(input.Connection);
        }

        public Collection<Input> input;
        System.Data.SQLite.SQLiteConnection connection;
        DataContext dataContext;

        public class Summary
        {
            public string Day;
            public TimeSpan Time;

            public TimeInterval Interval
            {
                get
                {
                    return new TimeInterval(Begin, End);
                }
                set { }
            }

            public double Key;
            public double Click;
            public double MouseMove;

            DateTime Begin
            {
                get
                {
                    return DateTime.Parse(Day);
                }
                set { }
            }

            DateTime End
            {
                get
                {
                    return Begin.AddDays(1);
                }
                set { }
            }

            public DateTime Come(DataContext dataContext)
            {
                return dataContext.ExecuteQuery<DateTime>("select Begin from input where Begin > {0} order by Begin limit 1", Day).First();
            }
            public DateTime Go(DataContext dataContext)
            {
                return dataContext.ExecuteQuery<DateTime>("select Begin from input where Begin < {0} order by Begin desc limit 1", DateTime.Parse(Day).AddDays(1)).First();
            }
        }

        [SQLiteFunction(Arguments = 2, FuncType = FunctionType.Scalar, Name = "Duration")]
        public class Duration : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                if (args.Length != 2)
                {
                    throw new IndexOutOfRangeException();
                }
                var s0 = (string)args[0];
                var s1 = (string)args[1];
                var d0 = DateTime.Parse(s0);
                var d1 = DateTime.Parse(s1);
                return (d1 - d0).TotalSeconds;
            }
        }

        IEnumerable<Summary> Days
        {
            get
            {
                string query = "select substr(Begin,0,11) as day, sum(Duration(Begin, End)) as Duration, sum(keydown) as Key, sum(clicks) as Click, sum(MouseMove) as MouseMove from input group by day";
                return dataContext.ExecuteQuery<Summary>(query, 0).ToList();
            }
        }

        [Usage("Prints a day-by-day report")]
        public void Report()
        {
            Report(Console.Out, TimeInterval.Last(new TimeSpan(30,0,0,0)));
        }

        public void Report(TextWriter w, TimeInterval range)
        {
            foreach (var i in range.Days)
            {
                var d = input.Range(i).ToList();

                w.WriteLine();
                w.WriteLine("{0:ddd yyyy-MM-dd}", i.Begin);
                var wis = d.WorkIntervals().ToList();
                foreach (var wi in wis)
                {
                    w.WriteLine("{0,-6} Begin: {2:HH:mm:ss} End: {3:HH:mm:ss} {1,6:F} h",
                        wi.Place,
                        wi.TimeInterval.Duration.TotalHours,
                        wi.TimeInterval.Begin,
                        wi.TimeInterval.End);
                }
                var homeSum = wis.Where(x => x.Place != Place.Office).Sum(x => x.TimeInterval.Duration.TotalHours);
                if (homeSum > 0)
                {
                    w.WriteLine("{0,-36} {1,6:F} h", "Home sum:", homeSum);
                }
            }
        }

        [Usage("Graphical reports")]
        public void GraphicalUserInterface()
        {
            Application.Run(StatisticsWindow());
        }

        public Form StatisticsWindow()
        {
            var main = new ListDetail();

            var all = new TimeInterval(input.First().Begin,input.Last().End);

            foreach (var mi in all.Months.Reverse())
            {
                TimeInterval m = mi;
                main.AddItem("{0:yyyy-MM} Overview".F(m.Begin), () =>
                {
                    var p = GraphEx.CreateTimeGraph();
                    p.XAxis.Scale.Min = new XDate(m.Begin).XLDate;
                    p.XAxis.Scale.Max = new XDate(m.End).XLDate;

                    p.YAxis.Type = AxisType.Date;
                    p.YAxis.Title.Text = "Day";

                    foreach (var i in Summarize(input.Range(m)))
                    {
                        var b = new BoxObj(
                            new XDate(i.Begin.Date).XLDate,
                            i.Begin.TimeOfDay.ToXDate().XLDate,
                            1.0,
                            (i.Begin - i.End).ToXDate().XLDate);
                        b.Fill = new Fill(i.TerminalServerSession ? Color.Red : Color.Green);
                        b.Border.IsVisible = false;
                        b.IsVisible = true;
                        b.IsClippedToChartRect = true;
                        p.GraphObjList.Add(b);
                    }

                    p.YAxis.Scale.Min = 0;
                    p.YAxis.Scale.Max = 1.0;

                    var c = p.AsControl();
                    c.ZoomEvent += new ZedGraphControl.ZoomEventHandler(c_ZoomEvent);

                    return c;
                });

                main.AddItem("{0:yyyy-MM} Hours".F(m.Begin), () =>
                {
                    var p = GraphEx.CreateTimeGraph();
                    p.XAxis.Scale.Min = new XDate(m.Begin).XLDate;
                    p.XAxis.Scale.Max = new XDate(m.End).XLDate;

                    p.BarSettings.Type = BarType.Stack;

                    var w = m.Days.Select(x => input.Range(x).WorkIntervals()).ToList();

                    p.AddBar(Place.Office.ToString(), PointList(w, x => x.Place == Place.Office), Color.Green);
                    p.AddBar(Place.OverHr.ToString(), PointList(w, x => x.Place == Place.OverHr), Color.Red);
                    p.AddBar(Place.Home.ToString(), PointList(w, x => x.Place == Place.Home), Color.Yellow);

                    return p.AsControl();
                });

                main.AddItem("{0:yyyy-MM} Activity".F(m.Begin), () =>
                {
                    var p = GraphEx.CreateTimeGraph();
                    p.XAxis.Scale.Min = new XDate(m.Begin).XLDate;
                    p.XAxis.Scale.Max = new XDate(m.End).XLDate;

                    p.BarSettings.Type = BarType.Stack;

                    var w = m.Days.Select(x => input.Range(x)).ToList();

                    p.AddBar("Local", PointList(w, x => !x.TerminalServerSession), Color.Green);
                    p.AddBar("Remote", PointList(w, x => x.TerminalServerSession), Color.Yellow);

                    return p.AsControl();
                });
            }

            foreach (var i in all.Days.Reverse())
            {
                var d = i;

                main.AddItem("{0} activity".F(i.Begin), () =>
                {
                    var p = GraphEx.CreateTimeGraph();
                    p.YAxisList.Clear();

                    var keystrokeAxis = new YAxis("keystrokes");
                    p.YAxisList.Add(keystrokeAxis);
                    keystrokeAxis.Scale.Min = 0;
                    keystrokeAxis.Scale.Max = 35e3;

                    var mouseAxis = new YAxis("mouse move");
                    p.YAxisList.Add(mouseAxis);
                    mouseAxis.Scale.Min = 0;
                    mouseAxis.Scale.Max = 6e6;

                    var data = input.Range(d);

                    var ppl = data.Select(x => new PointPair(new XDate(x.Begin), x.KeyDown)).ToPointPairList();
                    ppl = ppl.Accumulate();
                    var kp = p.AddCurve("keystrokes", ppl, Color.Black, SymbolType.None);
                    kp.YAxisIndex = 0;
                    

                    ppl = data.Select(x => new PointPair(new XDate(x.Begin), x.MouseMove)).ToPointPairList();
                    ppl = ppl.Accumulate();
                    var mc = p.AddCurve("mouse move", ppl, Color.Red, SymbolType.None);
                    mc.YAxisIndex = 1;

                    p.XAxis.Scale.Min = new XDate(d.Begin).XLDate;
                    p.XAxis.Scale.Max = new XDate(d.End).XLDate;

                    return p.AsControl();
                });
            }

            return main;
        }

        IPointList PointList(IEnumerable<IEnumerable<WorkInterval>> data, Func<WorkInterval, bool> which)
        {
            return data
                .Select(x =>
                {
                    if (x.Any())
                    {
                        var d = x.First().TimeInterval.Begin.Date;
                        var h = x.Where(which).Sum(wi => wi.TimeInterval.Duration.TotalHours);
                        return new PointPair(new XDate(d), h);
                    }
                    else
                    {
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToPointPairList();
        }

        IPointList PointList(IEnumerable<IEnumerable<Input>> data, Func<Input, bool> which)
        {
            return data
                .Select(x =>
                {
                    if (x.Any())
                    {
                        var d = x.First().Begin.Date;
                        var h = x.Where(which).Sum(wi => (wi.End - wi.Begin).TotalHours);
                        return new PointPair(new XDate(d), h);
                    }
                    else
                    {
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToPointPairList();
        }

        void c_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            var p = sender.MasterPane.PaneList[0];
            p.XAxis.Scale.Min = Math.Max(p.XAxis.Scale.Min, 0);
            p.XAxis.Scale.Max = Math.Min(p.XAxis.Scale.Max, 1.0);
        }

        IEnumerable<Input> Summarize(IEnumerable<Input> raw)
        {
            Input s = null;
            foreach (var i in raw)
            {
                if (s == null)
                {
                    s = new Input();
                    s.TerminalServerSession = i.TerminalServerSession;
                    s.Begin = i.Begin;
                    s.End = i.End;
                }
                else
                {
                    if (s.TerminalServerSession != i.TerminalServerSession || s.End != i.Begin)
                    {
                        yield return s;
                        s = null;
                    }
                    else
                    {
                        s.End = i.End;
                    }
                }
            }

            if (s != null)
            {
                yield return s;
            }
        }

        [TestFixture]
        public class Test
        {
            [Test, Explicit("interactive")]
            public void Stats()
            {
                Application.Run(new Program().StatisticsWindow());
            }

            [Test, Explicit("interactive")]
            public void Report()
            {
                new Program().Report(Console.Out, TimeInterval.LastDays(90));
            }

            [Test, Explicit("interactive")]
            public void OfficeReport()
            {
                var r = new Program();
                r.input = new Collection<Input>(@"D:\temp\2010-01-30_worktime\hagen\hagen.sqlite");
                Application.Run(r.StatisticsWindow());
            }
        }
    }
}
