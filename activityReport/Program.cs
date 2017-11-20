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
using System.Text.RegularExpressions;
using Sidi.Visualization;
using Sidi.Extensions;
using Sidi.Forms;
using L = Sidi.IO;
using Dvc = System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using OxyPlot.Axes;
using OxyPlot;
using OxyPlot.Series;

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
                new activityReport.Program(this.hagen).WorktimeReport(output, TimeIntervalExtensions.LastDays(180));
            }
            Process.Start("notepad.exe", p.ToString().Quote());
        }

        [Usage("Prints a day-by-day work time report")]
        public void Worktime()
        {
            // WorktimeReport(Console.Out, new TimeInterval(DateTime.Now.AddYears(-1), DateTime.Now));
            WorktimeReport(Console.Out, new TimeInterval(DateTime.Now.AddDays(-60), DateTime.Now));
        }

        [Usage("Prints a monthly report about overtime work")]
        public void Overtime()
        {
            OvertimeReport(Console.Out, new TimeInterval(DateTime.Now.AddDays(-60), DateTime.Now));
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

        IList<WorktimeInfo> GetWorktimeInfo(TimeInterval range)
        {
            var wti = range.Days.Select(x => new WorktimeInfo(x, input)).ToList();
            UpdateBalance(wti);
            return wti;
        }

        static string MonthString(TimeInterval i)
        {
            return String.Format("{0:yyyy-MM}", i.Begin);
        }

        public void WorktimeReport(TextWriter w, TimeInterval range)
        {
            var worktimeInfo = GetWorktimeInfo(range);
            var days = worktimeInfo.OrderByDescending(x => x.Date);

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

        public void OvertimeReport(TextWriter w, TimeInterval range)
        {
            var pauseTime = 0.75;
            var regularWorkTime = 8.0;
            var maximalWorkTime = 10.0;

            foreach (var month in range.Months)
            {
                w.WriteLine(@"

Work > {0} hours in {1:yyyy-MM}
====
", regularWorkTime, month.Begin);

                var worktimeInfo = GetWorktimeInfo(month);

                foreach (var i in worktimeInfo.Where(_ => _.Attendance != null))
                {
                    var worktime = i.Attendance.Duration.TotalHours - pauseTime;
                    var overtime = worktime - regularWorkTime;
                    if (overtime > 0)
                    {
                        var maxAllowedOvertime = maximalWorkTime - regularWorkTime;
                        overtime = overtime - 0.25 * ((int)((overtime - maxAllowedOvertime) / 0.25) + 1);
                        w.WriteLine("{0:ddd dd.MM.yyyy}: {1:F2} hours", i.Date, overtime);
                    }
                }
            }
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
        public void Statistics()
        {
            System.Windows.Forms.Application.Run(StatisticsWindow());
        }

        readonly Color idleColor = Color.Gray;
        readonly Color activeColor = Color.Red;
        
        public Form StatisticsWindow()
        {
            var main = new ListDetail();

            var all = new TimeInterval(input.First().Begin, input.Last().End);

            foreach (var mi in all.Months.Reverse())
            {
                TimeInterval m = mi;

                main.AddItem("{0:yyyy-MM} Overview".F(m.Begin), () => Overview(m));
                main.AddItem("{0:yyyy-MM} Hours".F(m.Begin), () => Hours(m));

                /*
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
                */
            }

            foreach (var iDay in all.Days.Reverse())
            {
                var day = iDay;
                main.AddItem("{0} activity".F(day.Begin), () => Activity(day));
            }

            main.List.FocusedItem = main.List.Items[0];
            main.List.Items[0].Selected = true;
            
            return main;
        }

        Control Activity(TimeInterval m)
        {
            var model = new OxyPlot.PlotModel()
            {
                Title = "Activity",
                Background = OxyColors.White,
                DefaultFont = "Arial",
                DefaultFontSize = 10.0
            };

            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = DateTimeAxis.ToDouble(m.Begin),
                Maximum = DateTimeAxis.ToDouble(m.End),
                StringFormat = "HH:mm"
            });

            var keyboardAxis = new OxyPlot.Axes.LinearAxis
            {
                Key = "keyboard",
                Title = "Keyboard",
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 60e3 * m.Duration.TotalDays
            };
            model.Axes.Add(keyboardAxis);

            var mouseAxis = new OxyPlot.Axes.LinearAxis
            {
                Key = "mouse",
                Title = "Mouse",
                Position = AxisPosition.Right,
                Minimum = 0,
                Maximum = 7000e3 * m.Duration.TotalDays
            };
            model.Axes.Add(mouseAxis);

            var keystrokes = new OxyPlot.Series.LineSeries
            {
                Title = "keystrokes",
                YAxisKey = keyboardAxis.Key
            };
            model.Series.Add(keystrokes);

            var mouse = new OxyPlot.Series.LineSeries
            {
                Title = "mouse",
                YAxisKey = mouseAxis.Key
            };
            model.Series.Add(mouse);

            var data = input.Range(m);
            int totalKeyDown = 0;
            double totalMouseMove = 0;
            foreach (var i in data)
            {
                totalKeyDown += i.KeyDown;
                totalMouseMove += i.MouseMove;
                keystrokes.Points.Add(new OxyPlot.DataPoint(DateTimeAxis.ToDouble(i.Begin), totalKeyDown));
                mouse.Points.Add(new OxyPlot.DataPoint(DateTimeAxis.ToDouble(i.Begin), totalMouseMove));
            }

            return new OxyPlot.WindowsForms.PlotView
            {
                Model = model
            };
        }

        const double daySep = 0.3;

        Control Overview(TimeInterval m)
        {
            var model = new OxyPlot.PlotModel()
            {
                Title = "Overview",
                Background = OxyColors.White,
                DefaultFont = "Arial",
                DefaultFontSize = 10.0
            };

            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MajorStep = 1,
                MinorStep = 1.0/4.0,
                Minimum = DateTimeAxis.ToDouble(m.Begin.AddDays(-1)),
                Maximum = DateTimeAxis.ToDouble(m.End),
                StringFormat = "ddd dd.MM.",
            });

            model.Axes.Add(new TimeSpanAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = TimeSpan.Zero.TotalSeconds,
                Maximum = TimeSpan.FromDays(1).TotalSeconds,
                StringFormat = "h:mm",
            });

            var bars = Summarize(input.Range(m));

            var activity = new OxyPlot.Series.RectangleBarSeries()
            {
                TrackerFormatString = "{2} - {3}",
                StrokeThickness = 0
            };

            foreach (var i in bars)
            {
                foreach (var t in i.TimeInterval.SplitToDays())
                {
                    var refDay = t.Begin.Date;
                    var y0 = DateTimeAxis.ToDouble(refDay);
                    var x0 = (t.Begin - refDay).TotalSeconds;
                    var x1 = (t.End - refDay).TotalSeconds;
                    activity.Items.Add(new OxyPlot.Series.RectangleBarItem(x0, y0 - daySep, x1, y0 + daySep)
                    {
                        Color = OxyPlot.OxyColor.FromUInt32((uint)(i.IsActive ? activeColor : idleColor).ToArgb())
                    });
                }
            }

            model.Series.Add(activity);

            /*
            var keys = new OxyPlot.Series.ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                        
            };

            foreach (var i in input.Range(m))
            {
                if (i.KeyDown > 0)
                {
                    keys.Points.Add(new OxyPlot.Series.ScatterPoint(
                        i.TimeInterval.Begin.ToLocalTime().TimeOfDay.TotalSeconds,
                        i.TimeInterval.Begin.ToLocalTime().Date.ToOADate()));
                }
            }

            model.Series.Add(keys);
            */

            var plotView = new OxyPlot.WindowsForms.PlotView
            {
                Model = model
            };

            return plotView;
        }

        Control Hours(TimeInterval m)
        {
            var model = new OxyPlot.PlotModel()
            {
                Title = "Hours",
                Background = OxyColors.White,
                DefaultFont = "Arial",
                DefaultFontSize = 10.0
            };

            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                // MinorGridlineStyle = LineStyle.Dot,
                MajorStep = 1,
                Minimum = DateTimeAxis.ToDouble(m.Begin.AddDays(-1)),
                Maximum = DateTimeAxis.ToDouble(m.End),
                StringFormat = "ddd dd.MM.",
            });

            model.Axes.Add(new TimeSpanAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "h:mm",
            });

            var w = input.Range(m)
                .GroupBy(x => x.TimeInterval.Begin.Date)
                .Select(g => new
                    {
                        Day = g.Key,
                        Active = g.Where(_ => _.IsActive).Sum(_ => _.TimeInterval.Duration.TotalSeconds),
                        On = g.Sum(_ => _.TimeInterval.Duration.TotalSeconds)
                    })
                    .ToList();

            var activity = new OxyPlot.Series.RectangleBarSeries()
            {
                TrackerFormatString = "{2} - {3}",
                StrokeThickness = 0
            };

            foreach (var i in w)
            {
                var refDay = i.Day;
                var y = DateTimeAxis.ToDouble(refDay);
                activity.Items.Add(new RectangleBarItem(0, y - daySep, i.On, y + daySep) { Color = OxyPlot.OxyColor.FromUInt32((uint)(idleColor).ToArgb())});
                activity.Items.Add(new RectangleBarItem(0, y - daySep, i.Active, y + daySep) { Color = OxyPlot.OxyColor.FromUInt32((uint)(activeColor).ToArgb())});
            }

            model.Series.Add(activity);

            var plotView = new OxyPlot.WindowsForms.PlotView
            {
                Model = model
            };

            return plotView;
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
        
        internal IEnumerable<Input> Summarize(IEnumerable<Input> raw)
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
    }
}
