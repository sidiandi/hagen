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
using Sidi.Persistence;

namespace hagen
{
    public class Log : Event
    {
        [Data]
        public string Level;

        [Data]
        public string Logger;

        [Data]
        public string Message;
    }

    public class Appender : log4net.Appender.AppenderSkeleton
    {
        Collection<Log> logs = Hagen.Instance.Logs;

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            var log = new Log();
            log.Level = loggingEvent.Level.ToString();
            log.Logger = loggingEvent.LoggerName;
            log.Message = loggingEvent.RenderedMessage;
            logs.Add(log);
        }
    }
}
