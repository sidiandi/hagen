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

namespace activityReport
{
    class Program
    {
        static void Main(string[] args)
        {
            SQLiteFunction.RegisterFunction(typeof(Duration));
            Parser.Run(new Program(), args);
        }

        public Program()
        {
            input = Collection<Input>.UserSetting();
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

            public double Duration
            {
                set
                {
                    Time = TimeSpan.FromSeconds(value);
                }
            }
                    
            public double Key;
            public double Click;
            public double MouseMove;

            public DateTime Begin
            {
                get
                {
                    return DateTime.Parse(Day);
                }
                set { }
            }

                public DateTime End
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

        [SQLiteFunction(Arguments=2, FuncType=FunctionType.Scalar, Name="Duration") ]
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
            foreach (var i in Days.ToList())
            {
                var d = input.Range(i.Begin, i.End).ToList();

                var come = d.FirstOrDefault(x => !x.TerminalServerSession);
                var go = d.LastOrDefault(x => !x.TerminalServerSession);

                var teleCommuting = come == null ? d : d.Where(x => x.End <= come.Begin || go.End <= x.Begin);
                var company = come == null ? new List<Input>() : d.Where(x => x.End > come.Begin || go.End > x.Begin);

                Console.WriteLine();
                var day = DateTime.Parse(i.Day);
                Console.WriteLine(day.ToString("ddd dd.MM.yyyy"));
                double teleCommutingTime = 0.0;
                double officeTime = 0.0;

                string format = "{0,-20}: {1}";
                string formatHours = "{0,-20}: {1:F2} h";
                string hoursComeGo = "{0:F2} h, come: {1:HH:mm:ss}, go: {2:HH:mm:ss}";
                if (come != null)
                {
                    officeTime = (go.End - come.Begin).TotalHours;
                    Console.WriteLine(format, "Company office", hoursComeGo.F(officeTime, come.Begin, go.End));
                    var active = company.Active().TotalHours;
                    // Console.WriteLine(format, "Active", "{0:F2} ({1:F0}%)".F(active, 100.0 * active / officeTime));
                }

                if (teleCommuting.Any())
                {
                    teleCommutingTime = teleCommuting.Active().TotalHours;
                    DateTime tcCome = go == null ? day.AddHours(8) : go.Begin.AddHours(1);
                    DateTime tcGo = tcCome.AddHours(teleCommutingTime);
                    Console.WriteLine(format, "Home office", "{0:F2} h, come: {1:HH:mm:ss}, go: {2:HH:mm:ss}".F(teleCommutingTime, tcCome, tcGo));
                }

                Console.WriteLine(formatHours, "Total", officeTime + teleCommutingTime);
            }
        }

        [Usage("Graphical reports")]
        public void GraphicalUserInterface()
        {
            var main = new ListDetail();

            foreach (var i in Days.ToList())
            {
                Summary s = i;
                main.AddItem(s.Day, () =>
                {
                    var p = GraphEx.CreateTimeGraph();
                    
                    var d = input.Range(s.Begin, s.End);

                    var ppl = new PointPairList();
                    ppl.AddRange(d.Select(x => new PointPair(new XDate(x.Begin), x.KeyDown)));
                    ppl = ppl.Accumulate();
                    p.AddCurve("keystrokes",ppl,Color.Black);
                    return p.AsControl();
                });
            }

            Application.Run(main);
        }

    }
}
