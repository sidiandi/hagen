using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Reflection;
using Sidi.Extensions;
using Sidi.CommandLine;

namespace hagen.ActionSource
{
    public class Plugins : Composite
    {
        public Plugins(PathList searchPath)
        {
            var assemblies = Find.AllFiles(searchPath)
                .Where(x => x.Extension.Equals(".dll") || x.Extension.Equals(".exe"));

            this.Sources = assemblies
                .SafeSelect(dll => Assembly.LoadFile(dll.FullName))
                .SelectMany(a => GetPlugins(a))
                .ToList();
        }

        static IList<IActionSource> GetPlugins(Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t.GetConstructor(new Type[] { }) != null)
                .ToList();

            return types
                .Where(t => typeof(IActionSource).IsAssignableFrom(t))
                .Select(t => (IActionSource)Activator.CreateInstance(t))

                .Concat(types
                    .Where(t => t.GetCustomAttributes(typeof(Usage), false).Any())
                    .Select(t =>
                    {
                        var parser = new Parser(Activator.CreateInstance(t));
                        return new ActionFilter(parser);
                    }))


                .ToList();
        }

        static IList<IActionSource> GetPlugins()
        {
            var assemblies = Sidi.IO.Paths.BinDir.GetFiles()
                .Where(x => x.Extension.Equals(".dll") || x.Extension.Equals(".exe"));

            return assemblies
                .SafeSelect(dll => Assembly.LoadFile(dll))
                .SelectMany(a => GetPlugins(a))
                .ToList();
        }

        public static IActionSource Default
        {
            get
            {
                return new Plugins(new PathList() { Sidi.IO.Paths.BinDir });
            }
        }
    }
}
