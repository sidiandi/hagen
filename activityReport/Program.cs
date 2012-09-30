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
using System.Text.RegularExpressions;
using Sidi.Visualization;
using Sidi.Extensions;

namespace activityReport
{
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log4net.Config.BasicConfigurator.Configure();
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

        [Usage("Prints a day-by-day work time report")]
        public void Worktime()
        {
            WorktimeReport(Console.Out, new TimeInterval(DateTime.Now.AddYears(-1), DateTime.Now));
        }

        public void Report(TextWriter w, TimeInterval range)
        {
            var wti = range.Days.Select(x => new WorktimeInfo(x, input)).ToList();
            foreach (var day in wti)
            {
                w.Write(@"{0:ddd dd.MM.yyyy}: ", day.Date);
                if (day.Come == null)
                {
                    w.Write("nicht anwesend");
                }
                else
                {
                    w.Write("Kommzeit {0:HH:mm} Uhr, Gehzeit {1:HH:mm} Uhr", day.Come, day.OfficialGo);
                }
                w.WriteLine();
            }
        }

        public class WorktimeInfo
        {
            public WorktimeInfo()
            {
                Come = null;
                Go = null;
            }

            public WorktimeInfo(TimeInterval day, Collection<Input> input)
            {
                var data = input.Range(day);
                Date = day.Begin.Date;

                Due = activityReport.Worktime.IsWorkDay(Date) ? activityReport.Worktime.RegularDailyWorkTime.TotalHours : 0;

                if (data.Any())
                {
                    Attendance = new TimeInterval(
                        data.First().Begin,
                        data.Last().End);
                    Come = Attendance.Begin;
                    Go = Attendance.End;

                    OfficialWorktime = activityReport.Worktime.GetOfficialWorkTime(Attendance).TotalHours;
                    RealWorktime = Attendance.Duration.TotalHours;
                    ActivityTime = data.Active().TotalHours;
                }
                else
                {
                    Attendance = null;
                    Come = null;
                    Go = null;
                    OfficialWorktime = 0;
                    RealWorktime = 0;
                    ActivityTime = 0;
                }
            }

            public DateTime? OfficialGo
            {
                get
                {
                    if (Go == null)
                    {
                        return null;
                    }

                    var lastLegalGo = this.Come 
                        + activityReport.Worktime.MaxWorkTimePerDay 
                        + activityReport.Worktime.Pause;

                    if (Go.Value < lastLegalGo)
                    {
                        return Go;
                    }
                    else
                    {
                        return lastLegalGo;
                    }

                }
            }

            public DateTime Date;
            public DateTime? Come;
            public DateTime? Go;
            public TimeInterval Attendance;
            public double OfficialWorktime;
            public double RealWorktime;
            public double ActivityTime;
            public double Due;
            public double Balance;

            public double Change { get { return OfficialWorktime - Due; } }
        }

        void UpdateBalance(IList<WorktimeInfo> e)
        {
            for (int i = 1; i < e.Count; ++i)
            {
                e[i].Balance = e[i - 1].Balance + e[i].OfficialWorktime - e[i].Due;
            }
        }

        public void WorktimeReport(TextWriter w, TimeInterval range)
        {
            var wti = range.Days.Select(x => new WorktimeInfo(x, input)).ToList();
            UpdateBalance(wti);

            wti.ListFormat()
                .AddColumn("Date", x => String.Format("{0:ddd dd.MM.yyyy}", x.Date))
                .AddColumn("Come", x => String.Format("{0:HH:mm}", x.Come))
                .AddColumn("Go", x => String.Format("{0:HH:mm}", x.Go))
                .AddColumn("Worktime", x => String.Format("{0:F2}", x.OfficialWorktime))
                .AddColumn("Due", x => String.Format("{0:F2}", x.Due))
                .AddColumn("Change", x => String.Format("{0:F2}", x.Change))
                .AddColumn("Balance", x => String.Format("{0:F2}", x.Balance))
                .RenderText();
        }

        [Usage("Program use")]
        public void ProgramUse()
        {
            var t = TimeInterval.LastDays(60);
            var programs = Hagen.Instance.ProgramUses.Query(p => p.Begin > t.Begin && p.Begin < t.End);
            log.Info(programs.Count);

            Sidi.Visualization.SimpleTreeMap.Show(programs
                .Select(x => new SimpleTreeMap.Item() { Lineage = Regex.Split(x.File, @"\\").Cast<object>(), Color = Color.White, Size = x.KeyDown }
                    ));
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

                main.AddItem("{0:yyyy-MM} Programs".F(m.Begin), () =>
                {
                    var programUse = Hagen.Instance.ProgramUses.Query(p => p.Begin > m.Begin && p.Begin < m.End && p.File != String.Empty);
                    var stm = new Sidi.Visualization.SimpleTreeMap();
                    stm.Items = 
                        programUse.Select(i => new Sidi.Visualization.SimpleTreeMap.Item()
                        {
                            Lineage = Regex.Split(i.File, @"\\"),
                            Size = (float) (i.End - i.Begin).TotalSeconds,
                            Color = Color.White,
                        });
                    return stm.CreateControl();
                });

                main.AddItem("{0:yyyy-MM} Captions".F(m.Begin), () =>
                {
                    var programUse = Hagen.Instance.ProgramUses.Query(p => p.Begin > m.Begin && p.Begin < m.End && p.Caption != String.Empty);
                    var stm = new Sidi.Visualization.SimpleTreeMap(); 
                    stm.Items =
                        programUse.Select(i => new Sidi.Visualization.SimpleTreeMap.Item()
                        {
                            Lineage = Regex.Split(i.Caption, @" \- ").Reverse(),
                            Size = (float)(i.End - i.Begin).TotalSeconds,
                            Color = Color.White,
                        });
                    return stm.CreateControl();
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
