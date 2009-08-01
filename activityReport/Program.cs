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

namespace activityReport
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Run(new Program(), args);
        }

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

        [Usage("Prints a day-by-day report")]
        public void Report()
        {
            SQLiteFunction.RegisterFunction(typeof(Duration));

            Collection<Input> input = Collection<Input>.UserSetting();

            var conn = (System.Data.SQLite.SQLiteConnection) input.Connection;

            string query = "select substr(Begin,0,11) as day, sum(Duration(Begin, End)) as Duration, sum(keydown) as Key, sum(clicks) as Click, sum(MouseMove) as MouseMove from input where TerminalServerSession = {0} group by day";

            DataContext d = new DataContext(input.Connection);

            Console.WriteLine("home office");
            foreach (var i in d.ExecuteQuery<Summary>(query, 1))
            {
                Console.WriteLine("{0}: {1}, {2}, {3}, {4}", i.Day, i.Time, i.Key, i.Click, i.MouseMove);
            }

            Console.WriteLine("company office");
            var companyDays = d.ExecuteQuery<Summary>(query, 0).ToList();
            foreach (var i in companyDays)
            {
                var come = d.ExecuteQuery<DateTime>("select Begin from input where Begin > {0} order by Begin limit 1", i.Day).First();
                var go = d.ExecuteQuery<DateTime>("select Begin from input where Begin < {0} order by Begin desc limit 1", DateTime.Parse(i.Day).AddDays(1)).First();
                Console.WriteLine("{0}: come {1}, go {2}, {3}, {4}, {5}, {6}", i.Day, come, go, i.Time, i.Key, i.Click, i.MouseMove);
            }
        }
    }
}
