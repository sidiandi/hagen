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

        Collection<Input> input;
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
                    w.WriteLine("{0,-6} Begin: {2:hh:mm:ss} End: {3:hh:mm:ss} {1,6:F} h",
                        wi.Place,
                        wi.TimeInterval.Duration.TotalHours,
                        wi.TimeInterval.Begin,
                        wi.TimeInterval.End);
                }
                var homeSum = wis.Where(x => x.Place == Place.Home).Sum(x => x.TimeInterval.Duration.TotalHours);
                if (homeSum > 0)
                {
                    w.WriteLine("{0,-36} {1,6:F} h", "Home sum:", homeSum);
                }

                /*
                var come = d.A.FirstOrDefault(x => !x.TerminalServerSession);
                var go = d.LastOrDefault(x => !x.TerminalServerSession);

                var teleCommuting = come == null ? d : d.Where(x => x.End <= come.Begin || go.End <= x.Begin);
                var company = come == null ? new List<Input>() : d.Where(x => x.End > come.Begin || go.End > x.Begin);

                w.WriteLine();
                var day = DateTime.Parse(i.Day);
                w.WriteLine(day.ToString("ddd dd.MM.yyyy"));
                double teleCommutingTime = 0.0;
                double officeTime = 0.0;
                double extraOfficeTime = 0.0;

                string format = "{0,-20}: {1}";
                string formatHours = "{0,-20}: {1:F2} h";
                string hoursComeGo = "{0:F2} h, come: {1:HH:mm:ss}, go: {2:HH:mm:ss}";
                if (come != null)
                {
                    officeTime = (go.End - come.Begin).TotalHours;
                    DateTime goTime = go.End;
                    DateTime extraComeTime = goTime;
                    DateTime extraGoTime = goTime;
                    if (officeTime > MaxWorkTime)
                    {
                        extraOfficeTime = officeTime - MaxWorkTime;
                        officeTime = MaxWorkTime;
                        goTime = come.Begin.AddHours(officeTime);
                        extraComeTime = goTime;
                        extraGoTime = go.End;
                    }
                    w.WriteLine(format, "Company office", hoursComeGo.F(officeTime, come.Begin, goTime));
                    if (extraOfficeTime > 0.0)
                    {
                        w.WriteLine(format, "Home office (x)", hoursComeGo.F(extraOfficeTime, extraComeTime, extraGoTime));
                    }
                    var active = company.Active().TotalHours;
                    // w.WriteLine(format, "Active", "{0:F2} ({1:F0}%)".F(active, 100.0 * active / officeTime));
                }

                if (teleCommuting.Any())
                {
                    teleCommutingTime = teleCommuting.Active().TotalHours;
                    var tcComeEvent = d.FirstOrDefault(x => x.TerminalServerSession && (go == null || go.End < x.Begin));
                    DateTime tcCome;
                    if (tcComeEvent != null)
                    {
                        tcCome = tcComeEvent.Begin;
                    }
                    else
                    {
                        tcCome = go == null ? day.AddHours(8) : go.Begin.AddHours(1);
                    }
                    DateTime tcGo = tcCome.AddHours(teleCommutingTime);
                    w.WriteLine(format, "Home office", "{0:F2} h, come: {1:HH:mm:ss}, go: {2:HH:mm:ss}".F(teleCommutingTime, tcCome, tcGo));
                }

                w.WriteLine(formatHours, "Total", officeTime + teleCommutingTime);
                 */
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
                main.AddItem("Overview {0:yyyy-MM} ".F(m.Begin), () =>
                {
                    var p = GraphEx.CreateTimeGraph();

                    p.YAxis.Type = AxisType.Date;
                    p.YAxis.Title.Text = "Day";

                    foreach (var i in Summarize(input.Range(m)))
                    {
                        var b = new BoxObj(
                            i.Begin.TimeOfDay.ToXDate().XLDate,
                            new XDate(i.Begin.Date).XLDate,
                            (i.Begin - i.End).ToXDate().XLDate,
                            1.0);
                        b.Fill = new Fill(i.TerminalServerSession ? Color.Red : Color.Green);
                        b.Border.IsVisible = false;
                        b.IsVisible = true;
                        b.IsClippedToChartRect = true;
                        p.GraphObjList.Add(b);
                    }

                    p.XAxis.Scale.Min = 0;
                    p.XAxis.Scale.Max = 1.0;

                    p.YAxis.Scale.Min = new XDate(m.Begin).XLDate;
                    p.YAxis.Scale.Max = new XDate(m.End).XLDate;

                    var c = p.AsControl();
                    c.ZoomEvent += new ZedGraphControl.ZoomEventHandler(c_ZoomEvent);

                    return c;
                });
            }

            foreach (var i in all.Days.Reverse())
            {
                var d = i;

                main.AddItem("{0} activity".F(i.Begin), () =>
                {
                    var p = GraphEx.CreateTimeGraph();
                    p.AddYAxis("keystrokes");
                    p.AddY2Axis("pixel");

                    var data = input.Range(d);

                    var ppl = data.Select(x => new PointPair(new XDate(x.Begin), x.KeyDown)).ToPointPairList();
                    ppl = ppl.Accumulate();
                    var kp = p.AddCurve("keystrokes", ppl, Color.Black, SymbolType.None);
                    kp.YAxisIndex = 1;
                    

                    ppl = data.Select(x => new PointPair(new XDate(x.Begin), x.MouseMove)).ToPointPairList();
                    ppl = ppl.Accumulate();
                    var mc = p.AddCurve("mouse move", ppl, Color.Red, SymbolType.None);
                    mc.YAxisIndex = 2;

                    return p.AsControl();
                });
            }

            return main;
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
        }
    }
}
