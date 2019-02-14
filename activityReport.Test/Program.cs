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
using OxyPlot.Axes;
using OxyPlot;
using OxyPlot.Series;

namespace activityReport.Test
{
    [TestFixture]
    public class ProgramTest : Sidi.Test.TestBase
    {
        [Test, Explicit("interactive")]
        public void Stats()
        {
            var hagen = new Hagen();
            var logDatabase = hagen.Context.GetService<ILogDatabase>();
            System.Windows.Forms.Application.Run(new activityReport.Program(logDatabase).StatisticsWindow());
        }

        [Test]
        public void Report()
        {
            var hagen = new Hagen();
            var logDatabase = hagen.Context.GetService<ILogDatabase>();
            new Program(logDatabase).Report(Console.Out, TimeIntervalExtensions.LastDays(30));
        }

        [Test, Explicit("interactive")]
        public void Summarize()
        {
            var hagen = new Hagen();
            var logDatabase = hagen.Context.GetService<ILogDatabase>();
            var p = new activityReport.Program(logDatabase);

            using (var inputs = logDatabase.OpenInputs())
            {
                var raw = inputs.Range(new TimeInterval(new DateTime(2015, 1, 8), new DateTime(2015, 1, 9)));
                raw.ListFormat()
                    .Add(_ => _.Begin, _ => _.End, _ => _.TimeInterval.Duration.TotalHours, _ => _.IsActive)
                    .RenderText();
                var sum = p.Summarize(raw);
                sum.ListFormat()
                    .Add(_ => _.Begin, _ => _.End, _ => _.TimeInterval.Duration.TotalHours, _ => _.IsActive).RenderText();
            }
        }

        [Test, Explicit("interactive")]
        public void OfficeReport()
        {
            var hagen = new Hagen();
            var logDatabase = hagen.Context.GetService<ILogDatabase>();
            var r = new Program(logDatabase);
            System.Windows.Forms.Application.Run(r.StatisticsWindow());
        }
    }
}
