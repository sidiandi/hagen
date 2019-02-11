using Sidi.IO;
using Sidi.Persistence;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    class LogDatabase : ILogDatabase
    {
        private readonly LPath logDatabasePath;

        public LogDatabase(LPath LogDatabasePath)
        {
            this.logDatabasePath = LogDatabasePath;
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
            return new Collection<Input>(logDatabasePath);
        }

        public Collection<ProgramUse> OpenProgramUses()
        {
            return new Collection<ProgramUse>(logDatabasePath);
        }

        public Collection<Log> OpenLogs()
        {
            return new Collection<Log>(logDatabasePath);
        }
    }
}
