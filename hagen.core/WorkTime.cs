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

using Sidi.Util;
using System;
using System.Linq;

namespace hagen
{
    internal class WorkTime : IWorkTime
    {
        private ILogDatabase logDatabase;

        public WorkTime(ILogDatabase logDatabase, IContract contract)
        {
            this.logDatabase = logDatabase;
            this.Contract = contract;
        }

        public IContract Contract { get; }

        public DateTime? GetWorkBegin(DateTime time)
        {
            var workDayBegin = time.Date;
            using (var inputs = logDatabase.OpenInputs())
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

    }
}