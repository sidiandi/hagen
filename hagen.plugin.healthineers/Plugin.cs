using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.plugin.healthineers
{
    class PluginFactory : IPluginFactory3
    {
        public IEnumerable<IPlugin3> CreatePlugins(IContext context) => new[] { new Plugin() };
    }

    class Plugin : ActionSourceCollector
    {
    }
}
