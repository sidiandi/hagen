using Amg.Build;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    class PluginFactory : IPluginFactory3
    {
        public IEnumerable<IPlugin3> CreatePlugins(IContext context)
        {
            var p = new Plugin();
            return new[] { p };
        }
    }

    class Plugin : IPlugin3
    {
        public IEnumerable<IActionSource3> GetActionSources()
        {
            return @"C:\src".Glob("*/.git")
                .Where(_ => _.IsDirectory())
                .Select(_ => _.Parent())
                .Select(_ => (IActionSource3)new SearchGitFilesNoGitProcess(_));
        }
    }
}
