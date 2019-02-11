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

namespace hagen
{
    public class Contract : IContract
    {
        public TimeSpan MaxWorkTimePerDay { get; } = TimeSpan.FromHours(10.75);
        public TimeSpan MaxHomeOfficeIdleTime { get; } = TimeSpan.FromMinutes(1);
        public TimeSpan RegularWorkTimePerWeek { get; } = TimeSpan.FromHours(40);
        public int WorkDaysPerWeek { get; } = 5;
        public TimeSpan PauseTimePerDay { get; } = TimeSpan.FromHours(0.75);
        public TimeSpan RegularWorkTimePerDay { get; } = TimeSpan.FromHours(8);
    }
}
