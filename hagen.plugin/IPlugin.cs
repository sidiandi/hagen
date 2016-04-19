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

    public interface IPlugin3
    {
        IEnumerable<IActionSource3> GetActionSources();
    }

    public interface IPluginFactory3
    {
        IEnumerable<IPlugin3> CreatePlugins(IContext context);
    }

    public static class IPluginExtensions
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
