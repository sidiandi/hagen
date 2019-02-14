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
