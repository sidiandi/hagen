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
using Sidi.Util;
using Sidi.CommandLine;
using hagen;
using Sidi.Persistence;
using Sidi.IO;
using System.Data.Linq;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Sidi.Visualization;
using Sidi.Extensions;
using Sidi.Forms;
using L = Sidi.IO;
using Dvc = System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;

namespace activityReport
{
    [Usage("Makes screen shots")]
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Parser.Run(new Program(new Hagen()), args);
        }

        Hagen hagen;

        public Program(Hagen hagen)
        {
            this.hagen = hagen;

            SQLiteFunction.RegisterFunction(typeof(Duration));
            input = hagen.OpenInputs();
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
            Report(Console.Out, TimeIntervalExtensions.LastDays(30));
        }

        [Usage("Shows a day-by-day worktime report")]
        public void ShowReport()
        {
            var p = hagen.DataDirectory.CatDir("work-time-report.txt");
            using (var output = new StreamWriter(p))
            {
                new activityReport.Program(this.hagen).WorktimeReport(output, TimeIntervalExtensions.LastDays(90));
            }
            Process.Start("notepad.exe", p.ToString().Quote());
        }

        [Usage("Show statistics window")]
        public void ShowStatisticsWindow()
        {
            StatisticsWindow().Show();
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

            var days = wti.OrderByDescending(x => x.Date);

            foreach (var i in days)
            {
                if (i.Date.DayOfWeek == DayOfWeek.Sunday)
                {
                    w.WriteLine(new string('-', 80));
                }

                w.WriteLine("{0:ddd dd.MM.yyyy}", i.Date);
                w.WriteLine("Kommzeit: {0:HH:mm}", i.Come);
                w.WriteLine("Gehzeit:  {0:HH:mm}", i.Go);
                w.WriteLine();
            }
            
            /*
            wti.ListFormat()
                .AddColumn("Date", x => String.Format("{0:ddd dd.MM.yyyy}", x.Date))
                .AddColumn("Come", x => String.Format("{0:HH:mm}", x.Come))
                .AddColumn("Go", x => String.Format("{0:HH:mm}", x.Go))
                .AddColumn("Worktime", x => String.Format("{0:F2}", x.OfficialWorktime))
                .AddColumn("Due", x => String.Format("{0:F2}", x.Due))
                .AddColumn("Change", x => String.Format("{0:F2}", x.Change))
                .AddColumn("Balance", x => String.Format("{0:F2}", x.Balance))
                .RenderText(w);
             */
        }

        TypedTreeMap<T> CreateTreeMap<T>(IList<T> data)
        {
            return new TypedTreeMap<T>()
            {
                Items = data
            };
        }

        [Usage("Program use")]
        public void ProgramUse()
        {
            using (var pu = hagen.OpenProgramUses())
            {
                var t = TimeIntervalExtensions.LastDays(60);
                var programs = pu.Query(p => p.Begin > t.Begin && p.Begin < t.End)
                    .GroupBy(x => x.File)
                    .Select(x => new { File = x.Key, KeyDown = x.Sum(i => i.KeyDown) })
                    .ToList();

                var tm = CreateTreeMap(programs);
                tm.GetLineage = x => Regex.Split(x.File.ToLower(), @"\\");
                tm.GetSize = x => x.KeyDown;

                tm.RunFullScreen();
            }
        }

        [Usage("Program use")]
        public void Captions()
        {
            using (var pu = hagen.OpenProgramUses())
            {
                var t = TimeIntervalExtensions.LastDays(60);
                var programs = pu.Query(p => p.Begin > t.Begin && p.Begin < t.End)
                    .GroupBy(x => x.Caption)
                    .Select(x => new { Caption = x.Key, KeyDown = x.Sum(i => i.KeyDown) })
                    .ToList();

                var tm = CreateTreeMap(programs);
                tm.GetLineage = x => Regex.Split(x.Caption, @" \- ").Reverse();
                tm.GetSize = x => x.KeyDown;

                tm.RunFullScreen();
            }
        }

        [Usage("Graphical reports")]
        public void GraphicalUserInterface()
        {
            System.Windows.Forms.Application.Run(StatisticsWindow());
        }

        readonly Color idleColor = Color.FromArgb(0xee, 0xee, 0xff);
        readonly Color activeColor = Color.Blue;
        
        public Form StatisticsWindow()
        {
            var main = new ListDetail();

            var all = new TimeInterval(input.First().Begin, input.Last().End);

            foreach (var mi in all.Months.Reverse())
            {
                TimeInterval m = mi;
                main.AddItem("{0:yyyy-MM} Overview".F(m.Begin), () =>
                {
                    var chart = new Dvc.Chart();

                    var ca = new Dvc.ChartArea()
                    {
                        Name = "Overview",
                    };
                    chart.ChartAreas.Add(ca);
                    
                    var active = new Dvc.Series()
                    {
                        Name = "Activity",
                        ChartType = Dvc.SeriesChartType.RangeBar,
                    };

                    chart.Series.Add(active);

                    var refday = m.Begin.Date;
                    ca.AxisY.Maximum = refday.AddDays(1).ToOADate();
                    ca.AxisY.Minimum = refday.ToOADate();

                    var stripLine = new StripLine()
                    {
                        IntervalOffsetType = DateTimeIntervalType.Days,
                        IntervalType = DateTimeIntervalType.Weeks,
                        StripWidthType = DateTimeIntervalType.Days,

                        Interval = 1,
                        StripWidth = 2,
                        BackColor = Color.LightGray,
                        IntervalOffset = -1.5,
                    };

                    ca.AxisX.StripLines.Add(stripLine);
                    ca.AxisX.LabelStyle.Format = "ddd dd.MM.yyyy";
                    ca.AxisX.IntervalType = DateTimeIntervalType.Days;
                    ca.AxisX.Interval = 1;

                    ca.AxisY.LabelStyle.Format = "HH:mm";
                    ca.AxisY.IntervalType = DateTimeIntervalType.Hours;
                    ca.AxisY.Interval = 1;

                    var bars = Summarize(input.Range(m));
                    log.Info(bars.ListFormat().AllPublic());
                    
                    foreach (var i in bars)
                    {
                        foreach (var t in i.TimeInterval.SplitToDays())
                        {
                            var y0 = (refday + t.Begin.TimeOfDay).ToOADate();
                            var y1 = y0 + t.Duration.TotalDays;
                            // log.DebugFormat("{0} {1} {2}", t, y0, y1);
                            var p = new DataPoint(t.Begin.Date.ToOADate(), new[] { y0, y1 });
                            p.Color = i.IsActive ? activeColor : idleColor;
                            active.Points.Add(p);
                        }
                    }

                    return chart;
                });

                main.AddItem("{0:yyyy-MM} Hours".F(m.Begin), () =>
                {
                    var c = new Dvc.Chart();

                    var ca = new Dvc.ChartArea("Activity");
                    c.ChartAreas.Add(ca);

                    var a = new Dvc.Series()
                    {
                        Name = "Activity",
                        ChartType = Dvc.SeriesChartType.StackedColumn
                    };

                    var t = new Dvc.Series()
                    {
                        Name = "Presence",
                        ChartType = Dvc.SeriesChartType.StackedColumn
                    };

                    var w = m.Days.Select(x => input.Range(x)).ToList();

                    ca.AxisY.Title = "Hours";

                    foreach (var i in w)
                    {
                        if (!i.Any())
                        {
                            continue;
                        }

                        var x = i.First().Begin.Date;
                        var active = i.Sum(j => (j.End - j.Begin).TotalHours);
                        var total = (i.Last().End - i.First().Begin).TotalHours;
                        total -= active;

                        a.Points.AddXY(x, active);
                        t.Points.AddXY(x, total);
                    }

                    c.Series.Add(a);
                    c.Series.Add(t);
                    
                    c.Legends.Add(new Dvc.Legend()
                    {
                        LegendStyle = Dvc.LegendStyle.Row,
                        Docking = Dvc.Docking.Top
                    });

                    return c;
                });

                main.AddItem("{0:yyyy-MM} Programs".F(m.Begin), () =>
                {
                    using (var pu = hagen.OpenProgramUses())
                    {
                        var programUse = pu
                            .Query(p => p.Begin > m.Begin && p.Begin < m.End && p.File != String.Empty)
                            .GroupBy(p => p.File)
                            .Select(p => new { File = new LPath(p.Key), TotalSeconds = p.Sum(x => (x.End - x.Begin).TotalSeconds) });

                        var stm = programUse.CreateTreeMap();
                        stm.GetLineage = x => x.File.Parts;
                        stm.GetSize = i => i.TotalSeconds;
                        stm.GetColor = i => Color.White;
                        stm.GetText = i => new LPath(i.File).FileName;

                        return stm;
                    }
                });

                main.AddItem("{0:yyyy-MM} Captions".F(m.Begin), () =>
                {
                    using (var pu = hagen.OpenProgramUses())
                    {
                        var programUse = pu
                        .Query(p => p.Begin > m.Begin && p.Begin < m.End && p.Caption != String.Empty)
                        .GroupBy(p => p.Caption)
                        .Select(p => new { Caption = Regex.Split(p.Key, @" \- ").Reverse(), Duration = p.Sum(x => (x.End - x.Begin).TotalSeconds)});

                        var tm = programUse.CreateTreeMap();
                        tm.GetLineage = x => x.Caption;
                        tm.GetSize = i => i.Duration;
                        tm.GetColor = i => Color.White;
                        tm.GetText = x => x.Caption.Last();
                        return tm;
                    }
                });
            }

            foreach (var iDay in all.Days.Reverse())
            {
                var day = iDay;
                main.AddItem("{0} activity".F(day.Begin), () =>
                {
                    var c = new Dvc.Chart();

                    var ca = new Dvc.ChartArea();
                    ca.AxisX.LabelStyle.Format = "HH:mm";

                    /*
                    ca.AxisY.Maximum = 35e3;
                    ca.AxisY2.Maximum = 6e6;
                     */
                    ca.AxisY.Title = "Keystrokes";
                    ca.AxisY2.Title  = "Mouse";

                    ca.AxisX.Minimum = day.Begin.ToOADate();
                    ca.AxisX.Maximum = day.End.ToOADate();
                    c.ChartAreas.Add(ca);


                    var keystrokes = new Dvc.Series()
                    {
                        Name = "keystrokes",
                        ChartType = Dvc.SeriesChartType.FastLine,
                    };

                    var mouse = new Dvc.Series()
                    {
                        Name = "mouse",
                        ChartType = Dvc.SeriesChartType.FastLine,
                        YAxisType = Dvc.AxisType.Secondary
                    };

                    c.Series.Add(keystrokes);
                    c.Series.Add(mouse);
                    
                    /*
                    keystrokeAxis.Scale.Min = 0;
                    keystrokeAxis.Scale.Max = 35e3;

                    var mouseAxis = new YAxis("mouse move");
                    p.YAxisList.Add(mouseAxis);
                    mouseAxis.Scale.Min = 0;
                    mouseAxis.Scale.Max = 6e6;
                    */

                    var data = input.Range(day);

                    int totalKeyDown = 0;
                    double totalMouseMove = 0;
                    foreach (var i in data)
                    {
                        totalKeyDown += i.KeyDown;
                        totalMouseMove += i.MouseMove;
                        keystrokes.Points.AddXY(i.Begin, totalKeyDown);
                        mouse.Points.AddXY(i.Begin, totalMouseMove);
                    }

                    c.Legends.Add(new Dvc.Legend()
                        {
                            Docking = Dvc.Docking.Top,
                            LegendStyle = Dvc.LegendStyle.Row,
                        });
                    return c;
                });
            }

            return main;
        }

        /*
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
        */

        void Add(ref Input sum, Input i)
        {
            if (sum == null)
            {
                sum = new Input();
                sum.TerminalServerSession = i.TerminalServerSession;
                sum.Begin = i.Begin;
            }
            sum.End = i.End;
            sum.Clicks += i.Clicks;
            sum.KeyDown += i.KeyDown;
            sum.MouseMove += i.MouseMove;
        }
        
        IEnumerable<Input> Summarize(IEnumerable<Input> raw)
        {
            Input s = null;
            foreach (var i in raw)
            {
                if (s != null)
                {
                    if (
                        s.TerminalServerSession != i.TerminalServerSession || 
                        s.End != i.Begin ||
                        s.IsActive != i.IsActive
                        )
                    {
                        yield return s;
                        s = null;
                    }
                }

                Add(ref s, i);
            }
        }

        [TestFixture]
        public class Test : Sidi.Test.TestBase
        {
            [Test, Explicit("interactive")]
            public void Stats()
            {
                System.Windows.Forms.Application.Run(new Program(new Hagen()).StatisticsWindow());
            }

            [Test]
            public void Report()
            {
                new Program(new Hagen()).Report(Console.Out, TimeIntervalExtensions.LastDays(30));
            }

            [Test, Explicit("interactive")]
            public void Summarize()
            {
                var hagen = new Hagen();
                var p = new Program(hagen);
                
                using (var inputs = hagen.OpenInputs())
                {
                    var raw = inputs.Range(new TimeInterval(new DateTime(2015, 1, 8), new DateTime(2015, 1, 9)));
                    raw.ListFormat()
                        .Add(_=> _.Begin, _=>_.End, _ => _.TimeInterval.Duration.TotalHours, _=>_.IsActive)
                        .RenderText();
                    var sum = p.Summarize(raw);
                    sum.ListFormat()
                        .Add(_=> _.Begin, _=>_.End, _ => _.TimeInterval.Duration.TotalHours, _=>_.IsActive).RenderText();
                }
            }

            [Test, Explicit("interactive")]
            public void OfficeReport()
            {
                var r = new Program(new Hagen());
                r.input = new Collection<Input>(@"D:\temp\2010-01-30_worktime\hagen\hagen.sqlite");
                System.Windows.Forms.Application.Run(r.StatisticsWindow());
            }
        }
    }
}
