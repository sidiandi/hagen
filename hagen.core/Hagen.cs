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

namespace hagen
{
    public class Hagen
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Hagen()
        {
            actions = OpenActions();
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

        public LPath CaptureActiveWindow()
        {
            var sc = new ScreenCapture();
            var dir = ScreenCaptureDirectory;
            var fe = UserInterfaceState.Instance.SavedFocusedElement;
            if (fe == null)
            {
                throw new Exception("Not active window");
            }
            return sc.CaptureWindow(dir, fe.GetTopLevelElement());
        }

        public IList<LPath> CaptureScreens()
        {
            var sc = new ScreenCapture();
            var dir = ScreenCaptureDirectory;
            return sc.CaptureAll(dir);
        }

        public LPath CapturePrimaryScreen()
        {
            var sc = new ScreenCapture();
            var dir = ScreenCaptureDirectory;
            return sc.CaptureToDirectory(Screen.PrimaryScreen, dir);
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
        class InputL
        {
            public DateTime End { set; get;}
            public DateTime Begin { set; get; }
            public int KeyDown { set; get; }
            public double MouseMove { set; get; }
            public int Clicks { set; get; }
            public bool TerminalServerSession { set; get; }
        }
    
        public static IEnumerable<Input> Range(this Collection<Input> inputs, TimeInterval range)
        {
            string q = "select oid as Id, * from input where begin >= {0} and begin <= {1}".F(
                range.Begin.ToString(dateFmt).Quote(),
                range.End.ToString(dateFmt).Quote());

            var cmd = inputs.Connection.CreateCommand();
            cmd.CommandText = q;

            var dc = new DataContext(inputs.Connection);
            return dc.Translate<InputL>(cmd.ExecuteReader()).Select(x =>
                {
                    var y = new Input();
                    y.End = x.End;
                    y.Begin = x.Begin;
                    y.KeyDown = x.KeyDown;
                    y.MouseMove = x.MouseMove;
                    y.Clicks = x.Clicks;
                    y.TerminalServerSession = x.TerminalServerSession;
                    return y;
                }).ToList();
        }

        const string dateFmt = "yyyy-MM-dd HH:mm:ss";

    }
}
