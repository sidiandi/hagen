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
using Sidi.Persistence;
using Sidi.Util;

namespace hagen
{
    public class Event
    {
        public Event()
        {
            Begin = DateTime.Now;
        }

        [RowId]
        public long Id;

        [Data, Indexed]
        public DateTime Begin { set; get; }
    }

    public class Activity : Event
    {
        public Activity()
        {
            End = Begin;
        }

        [Data]
        public DateTime End { set; get; }

        public TimeInterval TimeInterval
        {
            get
            {
                return new TimeInterval(Begin, End);
            }
        }
    }

    public class Input : Activity
    {
        [Data]
        public int KeyDown;

        [Data]
        public double MouseMove;

        [Data]
        public int Clicks;

        [Data]
        public bool TerminalServerSession;

        public bool IsActive
        {
            get
            {
                return KeyDown > 0;
            }
        }
    }

    public class ProgramUse : Input
    {
        [Data]
        public string File;

        [Data]
        public string Caption;
    }
}
