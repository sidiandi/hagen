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
    public class PluginProvider : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PluginProvider(IContext context, PathList searchPath)
        {
            this.context = context;

            this.Plugins = GetPlugins(searchPath);
        }

        readonly IContext context;

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
                .Create<IPluginFactory>()
                .SelectMany(f => f.CreatePlugins(context))
                .ToList();
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var p in Plugins.OfType<IDisposable>())
                    {
                        p.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PluginProvider() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
