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

using Sidi.IO;
using System;
using Sidi.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.Data.Linq;

namespace hagen
{
    public class Hagen
    {
        public string DatabasePath
        {
            get
            {
                return FileUtil.CatDir(DataDirectory, "hagen.sqlite");
            }
        }

        public string DataDirectory
        {
            get
            {
                return FileUtil.CatDir(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "hagen");
            }
        }

        public Collection<Action> Actions
        {
            get
            {
                return new Collection<Action>(DatabasePath);
            }
        }

        public DateTime? GetWorkBegin(DateTime time)
        {
            var workDayBegin = time.Date;
            var r = Inputs.Range(new TimeInterval(workDayBegin, time));
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

        public Collection<Input> Inputs
        {
            get
            {
                return new Collection<Input>(DatabasePath);
            }
        }

        public Collection<ProgramUse> ProgramUses
        {
            get
            {
                return new Collection<ProgramUse>(DatabasePath);
            }
        }

        public Collection<Log> Logs
        {
            get
            {
                return new Collection<Log>(DatabasePath);
            }
        }

        public static Hagen Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Hagen();
                }
                return instance;
            }
        }

        static Hagen instance;
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
