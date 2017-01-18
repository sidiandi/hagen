using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    [Obsolete]
    internal interface IPluginFactory
    {
        IEnumerable<IPlugin> CreatePlugins(IContext context);
    }

    [Obsolete]
    internal interface IPlugin
    {
        IEnumerable<IActionSource2> GetActionSources();
    }

    [Obsolete]
    internal static class IPluginExtensions
    {
        public static IPlugin3 ToIPlugin3(this IPlugin plugin)
        {
            return new Wrapper(plugin);
        }

        class Wrapper : IPlugin3
        {
            IPlugin plugin;

            public Wrapper(IPlugin plugin)
            {
                this.plugin = plugin;
            }

            public IEnumerable<IActionSource3> GetActionSources()
            {
                return plugin.GetActionSources().Select(_ => _.ToActionSource3());
            }
        }
    }
}
