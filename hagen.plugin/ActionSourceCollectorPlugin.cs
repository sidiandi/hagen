using Sidi.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace hagen
{
    /// <summary>
    /// Derive from this class to collect all action sources in your assembly
    /// </summary>
    public class ActionSourceCollector : IPlugin3
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IContext Context { get; set; }

        public virtual void Init(IContext context)
        {
            Context = context;
            GetActionSources();
        }

        T Create<T>(Type t)
        {
            var contextCtor = t.GetConstructor(new Type[] { typeof(IContext) });
            if (contextCtor != null)
            {
                return (T) contextCtor.Invoke(new object[] { this.Context });
            }

            var defaultCtor = t.GetConstructor(new Type[] { });
            if (defaultCtor != null)
            {
                return (T) defaultCtor.Invoke(new object[] { });
            }

            return default(T);
        }

        public virtual IEnumerable<IActionSource3> GetActionSources()
        {
            if (_actionSources == null)
            {
                _actionSources = GetActionSourcesImpl().ToList();
            }
            return _actionSources;
        }

        private List<IActionSource3> _actionSources;

        IEnumerable<IActionSource3> GetActionSourcesImpl()
        {
            var assembly = GetType().Assembly;
            var types = assembly.GetTypes();

            var actionSources = types
                .Where(t => !t.Name.StartsWith("Test_"))
                .Select(t =>
                    {
                        if (typeof(IActionSource3).IsAssignableFrom(t))
                        {
                            return Create<IActionSource3>(t);
                        }
                        else if (typeof(IActionSource2).IsAssignableFrom(t))
                        {
                            return Create<IActionSource2>(t).ToActionSource3();
                        } 
                        else if (typeof(IActionSource).IsAssignableFrom(t))
                        {
                            var a = Create<IActionSource>(t);
                            if (a == null)
                            {
                                return null;
                            }
                            return a.ToIActionSource2().ToActionSource3();
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
                        object plugin = Create<object>(t);
                        if (plugin == null)
                        {
                            return null;
                        }

                        var parser = Parser.SingleSource(plugin);
                        return new CommandLineParserActionSource(this.Context, parser);
                    })
                    .Where(x => x != null)
                    )

                .ToList();

            log.InfoFormat("Action sources found in {0}\r\n{1}", assembly, actionSources.ListFormat());
            return actionSources;
        }
    }
}
