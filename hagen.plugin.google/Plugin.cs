using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.plugin.google
{
    class PluginFactory : IPluginFactory3
    {
        public IEnumerable<IPlugin3> CreatePlugins(IContext context)
        {
            yield return new Plugin(context);
        }
    }

    class Plugin : IPlugin3
    {
        IContext context;
        readonly IActionSource3[] actionSources;

        public Plugin(IContext context)
        {
            this.context = context;
            this.actionSources = new IActionSource3[] { new GoogleContacts(context) };
        }

        IEnumerable<IActionSource3> IPlugin3.GetActionSources()
        {
            return actionSources;
        }
    }
}
