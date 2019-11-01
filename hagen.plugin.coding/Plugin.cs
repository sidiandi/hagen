using Amg.Build;
using Amg.FileSystem;
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

    class Plugin : ActionSourceCollector
    {
        public override IEnumerable<IActionSource3> GetActionSources()
        {
            var found = base.GetActionSources();

            var gitSearch = @"C:\src".Glob("*/.git")
                .Where(_ => _.IsDirectory())
                .Select(_ => _.Parent())
                .Select(_ => (IActionSource3)new SearchGitFilesNoGitProcess(_));

            var actionSources = found.Concat(gitSearch).ToList();
            return actionSources;
        }
    }
}
