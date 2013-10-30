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
        public Plugins(Hagen hagen, PathList searchPath)
        {
            this.hagen = hagen;

            var hagenExe = Assembly.GetEntryAssembly().GetLocalPath();

            var assemblyFileExtension = new Sidi.IO.FileType("exe", "dll");
            var assemblyFiles = Find.AllFiles(searchPath)
                .Select(x => x.FullName)
                .Where(x => assemblyFileExtension.Is(x))
                .Where(x => !x.Equals(hagenExe))
                .ToList();

            this.Sources = assemblyFiles
                .SafeSelect(dll => Assembly.LoadFile(dll))
                .SelectMany(a => GetPlugins(a))
                .ToList();
        }

        Hagen hagen;

        IList<IActionSource> GetPlugins(Assembly assembly)
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
                        object plugin = null;
                        var hagenCtor = t.GetConstructor(new []{ typeof(Hagen) } );
                        if (hagenCtor == null)
                        {
                            var defaultCtor = t.GetConstructor(new Type[]{});
                            plugin = defaultCtor.Invoke(new object[]{});
                        }
                        else
                        {
                            plugin = hagenCtor.Invoke(new object[]{hagen});
                        }

                        var parser = new Parser(plugin);
                        return new ActionFilter(parser);
                    }))


                .ToList();
        }

        IList<IActionSource> GetPlugins()
        {
            var extensions = new Sidi.IO.FileType("exe", "dll");
            var assemblies = Sidi.IO.Paths.BinDir.GetFiles()
                .Where(x => extensions.Is(x))
                .Where(x => !x.FileName.Equals("hagen.exe"));

            return assemblies
                .SafeSelect(dll => Assembly.LoadFile(dll))
                .SelectMany(a => GetPlugins(a))
                .ToList();
        }

        public static IActionSource GetDefaultPlugins(Hagen hagen)
        {
            return new Plugins(hagen, new PathList() { Sidi.IO.Paths.BinDir });
        }
    }
}
