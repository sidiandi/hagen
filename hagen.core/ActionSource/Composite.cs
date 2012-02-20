using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace hagen.ActionSource
{
    public class Composite : IActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Composite(params IActionSource[] sources)
        {
            this.Sources = sources.ToList();
        }

        public IList<IAction> GetActions(string query)
        {
            return Sources.SelectMany(source => 
                {
                    try
                    {
                        return source.GetActions(query);
                    }
                    catch (Exception ex)
                    {
                        log.Warn(
                            String.Format("{0} : query={1}", source, query),
                            ex);
                        return new IAction[] { };
                    }
                })
                .ToList();
        }

        public IList<IActionSource> Sources;

        public static IList<IActionSource> GetPlugins(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => typeof(IActionSource).IsAssignableFrom(t))
                .Where(t => t.GetConstructor(new Type[]{}) != null)
                .Select(t => (IActionSource) Activator.CreateInstance(t))
                .ToList();
        }

        public static IList<IActionSource> GetPlugins()
        {
            var dlls = System.IO.Directory.GetFiles(Sidi.IO.FileUtil.BinFile("."), "*.dll");
            
            return dlls
                .Select(dll => Assembly.LoadFile(dll))
                .SelectMany(a => GetPlugins(a))
                .ToList();
        }

        public static IActionSource Plugins
        {
            get
            {
                return new Composite() { Sources = GetPlugins() };
            }
        }
    }
}
