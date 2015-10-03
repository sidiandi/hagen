using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    class PluginFactory : IPluginFactory
    {
        public IEnumerable<IPlugin> CreatePlugins(IContext context)
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
