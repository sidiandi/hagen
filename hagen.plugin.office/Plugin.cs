using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.plugin.office
{
    class PluginFactory : IPluginFactory3
    {
        public IEnumerable<IPlugin3> CreatePlugins(IContext context)
        {
            var p = new Plugin();
            p.Init(context);
            return new[] { p };
        }
    }

    class Plugin : ActionSourceCollector
    {
    }
}
