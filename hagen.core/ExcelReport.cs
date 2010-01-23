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
using System.Reflection;
using NUnit.Framework;
using Sidi.Persistence;
using Sidi.Util;

namespace hagen
{
    public class ExcelCursor
    {
        public int Row {get; set;}
        public int Column { set; get; }
        Excel.Range range;

        public ExcelCursor(Excel.Range range)
        {
            this.range = range;
            Row = 1;
            Column = 1;
        }

        public void NextRow()
        {
            ++Row;
            Column = 1;
        }

        public object Value
        {
            set
            {
                range[Row, Column] = value;
                ++Column;
            }
        }
    }

    public class ExcelReport
    {
        public static Missing missing = Missing.Value;
        public void TimeReportByDays()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var a = new Microsoft.Office.Interop.Excel.Application();
            a.Visible = true;
            var wb = a.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
            var ws = (Excel.Worksheet) wb.Worksheets[1];
            var c = new ExcelCursor(ws.Cells);

            DateTime today = DateTime.Now.Date;

            c.Value = "Date";
            c.Value = "Begin";
            c.Value = "End";
            c.Value = "Present";
            c.Value = "Active";
            c.Value = "Keydown";
            c.Value = "Clicks";
            c.Value = "MouseMove";
            c.NextRow();

            for (DateTime i = today.AddDays(-30); i <= today; i = i.AddDays(1))
            {
                c.Value = i.Date;

                var inputs = Hagen.Instance.Inputs.Select(
                    "Begin >= {0} and End <= {1}".F(
                    i.ToString("yyyy-MM-dd").Quote(),
                    i.AddDays(1).ToString("yyyy-MM-dd").Quote()));

                if (inputs.Any())
                {
                    var begin = inputs.Select(x => x.Begin).Min();
                    c.Value = begin;
                    var end = inputs.Select(x => x.End).Max();
                    c.Value = end;
                    c.Value = (end - begin).TotalHours;
                    c.Value = inputs.Sum(x => (x.End - x.Begin).TotalHours);
                    c.Value = inputs.Sum(x => x.KeyDown);
                    c.Value = inputs.Sum(x => x.Clicks);
                    c.Value = inputs.Sum(x => x.MouseMove);
                }

                c.NextRow();
            }
            ws.Columns.AutoFit();
        }
    }

    [TestFixture]
    public class ExcelReportTest
    {
        [Test]
        public void TimeReportByDays()
        {
            var i = new ExcelReport();
            i.TimeReportByDays();
        }

        [Test]
        public void CreateOutlookAppointments()
        {
            var a = new Outlook.Application();

            Outlook.AppointmentItem appointment = (Outlook.AppointmentItem) a.CreateItem(Outlook.OlItemType.olAppointmentItem);

            appointment.Subject = "Feierabend";
            appointment.Start = DateTime.Now;
            appointment.End = appointment.Start.Date.AddDays(1);
            appointment.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
            appointment.ReminderMinutesBeforeStart = 15;
            appointment.ReminderSet = true;
            appointment.Save();
            
        }
    }
}
