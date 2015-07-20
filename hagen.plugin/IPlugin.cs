using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public interface IPlugin
    {
        void Init(IContext context);
        IEnumerable<IActionSource2> GetActionSources();
    }
}
