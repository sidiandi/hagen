using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public interface ILogDatabase
    {
        Sidi.Persistence.Collection<Input> OpenInputs();
        Sidi.Persistence.Collection<ProgramUse> OpenProgramUses();
        Sidi.Persistence.Collection<Log> OpenLogs();
    }
}
