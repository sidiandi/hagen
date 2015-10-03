using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.plugin.google
{
    class PluginFactory : IPluginFactory
    {
        public IEnumerable<IPlugin> CreatePlugins(IContext context)
        {
            return new IPlugin[] { new Plugin(context) };
        }
    }

    class Plugin : IPlugin
    {
        IContext context;

        public Plugin(IContext context)
        {
            this.context = context;
        }

        public IEnumerable<IActionSource2> GetActionSources()
        {
            return new IActionSource2[] { new GoogleContacts(context) };
        }
    }
}
