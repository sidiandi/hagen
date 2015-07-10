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
using Sidi.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.Data.Linq;
using Sidi.IO;
using Sidi.Extensions;
using System.Windows.Forms;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using System.Data.Common;

namespace hagen
{
    public class Hagen
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Hagen()
        {
            actions = OpenActions();
            this.Context = new UserInterfaceState();
        }

        Collection<Action> actions;

        public LPath DatabasePath
        {
            get
            {
                return DataDirectory.CatDir("hagen.sqlite");
            }
        }

        public LPath DataDirectory
        {
            get
            {
                return Sidi.IO.Paths.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    .CatDir("hagen");
            }
        }

        public LPath ScreenCaptureDirectory
        {
            get
            {
                return DataDirectory.CatDir("screen");
            }
        }

        public Collection<Action> OpenActions()
        {
            return new Collection<Action>(DatabasePath);
        }

        public UserInterfaceState Context { get; private set; }

        public void Cleanup()
        {
            using (var actions = OpenActions())
            {
                actions.Cleanup();
            }
        }
        
        public DateTime? GetWorkBegin(DateTime time)
        {
            var workDayBegin = time.Date;
            using (var inputs = OpenInputs())
            {
                var r = inputs.Range(new TimeInterval(workDayBegin, time));
                var b = r.FirstOrDefault();
                if (b == null)
                {
                    return null;
                }
                else
                {
                    return b.Begin;
                }
            }
        }

        public Collection<Input> OpenInputs()
        {
            return new Collection<Input>(actions.SharedConnection);
        }

        public Collection<ProgramUse> OpenProgramUses()
        {
            return new Collection<ProgramUse>(actions.SharedConnection);
        }

        public Collection<Log> OpenLogs()
        {
            return new Collection<Log>(actions.SharedConnection);
        }
    }

    public static class HagenEx
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        class InputL
        {
            public DateTime End { set; get;}
            public DateTime Begin { set; get; }
            public long KeyDown { set; get; }
            public long MouseMove { set; get; }
            public long Clicks { set; get; }
            public bool TerminalServerSession { set; get; }
        }
    
        public static IEnumerable<TimeInterval> SplitToDays(this TimeInterval interval)
        {
            for (var i = interval.Begin; i < interval.End;)
            {
                var e = i.Date.AddDays(1);
                if (e > interval.End)
                {
                    e = interval.End;
                }
                yield return new TimeInterval(i, e);
                i = e;
            }
        }
        
        public static IEnumerable<Input> Range(this Collection<Input> inputs, TimeInterval range)
        {
            string q = "select Begin, End, KeyDown, MouseMove, Clicks, TerminalServerSession from input where begin >= {0} and end <= {1}".F(
                range.Begin.ToString(dateFmt).Quote(),
                range.End.ToString(dateFmt).Quote());

            using (var cmd = inputs.Connection.CreateCommand())
            {
                cmd.CommandText = q;

                using (var r = cmd.ExecuteReader())
                {
                    return ReadInputs(r).ToList();
                }
            }
        }

        static IEnumerable<Input> ReadInputs(DbDataReader r)
        {
            for (; r.Read();)
            {
                var input = new Input();
                input.Begin = r.GetDateTime(0);
                input.End = r.GetDateTime(1);
                input.KeyDown = (int) r.GetInt64(2);
                input.MouseMove = r.GetDouble(3);
                input.Clicks = (int) r.GetInt64(4);
                input.TerminalServerSession = r.GetBoolean(5);
                yield return input;
            }
        }

        const string dateFmt = "yyyy-MM-dd HH:mm:ss";

    }
}
