﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Reflection;
using Sidi.Extensions;
using Sidi.CommandLine;

namespace hagen.ActionSource
{
    public class PluginProvider
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PluginProvider(Hagen hagen, PathList searchPath)
        {
            this.hagen = hagen;

            this.Plugins = GetPlugins(searchPath);
        }

        Hagen hagen;

        public IList<IPlugin> Plugins { get; private set; }

        public IEnumerable<IActionSource2> GetActionSources()
        {
            return Plugins.SelectMany(p => p.GetActionSources())
                .Where(x => x != null)
                .ToList();
        }

        IList<IPlugin> GetPlugins(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !object.Equals(typeof(IPlugin), t))
                .Select(t =>
                    {
                        var plugin = (IPlugin) t.GetConstructor(new Type[]{}).Invoke(new object[]{});
                        plugin.Init(hagen.Context);
                        return plugin;
                    })
                .ToList();

           /*
            return types
                .Where(t => !t.Name.StartsWith("Test_"))
                .Select(t =>
                    {
                        if (typeof(IActionSource2).IsAssignableFrom(t))
                        {
                            var ctor = t.GetConstructor(new Type[] { });

                            if (ctor == null)
                            {
                                return null;
                            }

                            return (IActionSource2)ctor.Invoke(new object[] { });
                        } 
                        else if (typeof(IActionSource).IsAssignableFrom(t))
                        {
                            var ctor = t.GetConstructor(new Type[] { });
                        
                            if (ctor == null)
                            {
                                return null;
                            }

                            return ((IActionSource)ctor.Invoke(new object[] { })).ToIActionSource2();
                        }
                        else
                        {
                            return null;
                        }
                    })
                .Where(t => t != null)

                .Concat(types
                    .Where(t => t.GetCustomAttributes(typeof(Usage), false).Any())
                    .Select(t =>
                    {
                        object plugin = null;
                        var hagenCtor = t.GetConstructor(new Type[]{ typeof(Hagen) } );
                        if (hagenCtor != null)
                        {
                            plugin = hagenCtor.Invoke(new object[] { hagen });
                            goto ok;
                        }

                        var defaultCtor = t.GetConstructor(new Type[] { });
                        if (defaultCtor != null)
                        {
                            plugin = defaultCtor.Invoke(new object[] { });
                            goto ok;
                        }

                        return null;

                    ok:
                        var parser = Parser.SingleSource(plugin);
                        return new ActionFilter(parser);
                    })
                    .Where(x => x != null)
                    )

                .ToList();
             */
        }

        IList<IPlugin> GetPlugins(PathList searchPath)
        {
            var assemblyFileExtension = new Sidi.IO.FileType("exe", "dll");
            var hagenExe = Assembly.GetEntryAssembly().GetLocalPath();
            var sidiUtil = typeof(Sidi.IO.LPath).Assembly.GetLocalPath();

            var assemblyFiles = searchPath
                .SelectMany(x => x.GetFiles())
                .Where(x => assemblyFileExtension.Is(x))
                .Where(x => !x.Equals(hagenExe) && !x.Equals(sidiUtil))
                .ToList();

            return assemblyFiles
                .Select(dll => { try { return Assembly.LoadFile(dll); } catch { return null; } })
                .Where(x => x != null)
                .SelectMany(a => GetPlugins(a))
                .ToList();
        }
    }
}
