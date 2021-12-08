using Amg.FileSystem;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace hagen.plugin.healthineers
{
    class PluginFactory : IPluginFactory3
    {
        public IEnumerable<IPlugin3> CreatePlugins(IContext context) => new[] { new Plugin(context) };
    }

    class Plugin : ActionSourceCollector
    {
        public Plugin(IContext context)
        {
            Init(context);
        }

        public override IEnumerable<IActionSource3> GetActionSources()
        {
            // var notesDir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Combine("meetings");
            var notesDir = @"c:\src\meetings";
            return base.GetActionSources().Concat(new IActionSource3[] { new FastNotes(notesDir) });
        }
    }
}
