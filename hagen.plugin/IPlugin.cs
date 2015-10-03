using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public interface IPluginFactory
    {
        IEnumerable<IPlugin> CreatePlugins(IContext context);
    }

    public interface IPlugin
    {
        IEnumerable<IActionSource2> GetActionSources();
    }
}
