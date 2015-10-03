using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public interface IExecutionTimeLog
    {
        DateTime GetLastExecutionTime(string id);

        void SetLastExecutionTime(string id);
    }
}
