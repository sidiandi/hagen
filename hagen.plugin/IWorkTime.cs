using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public interface IWorkTime
    {
        DateTime? GetWorkBegin(DateTime now);
        IContract Contract { get; }
    }
}
