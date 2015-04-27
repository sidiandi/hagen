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
    public class ActionSourceCollector : IPlugin
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IContext Context { get; set; }

        public virtual void Init(IContext context)
        {
            Context = context;
        }

        T Create<T>(Type t)
        {
            object plugin = null;
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

        public virtual IEnumerable<IActionSource2> GetActionSources()
        {
            var assembly = GetType().Assembly;
            log.InfoFormat("Looking for action sources in {0}", assembly);
            var types = assembly.GetTypes();

            var actionSources = types
                .Where(t => !t.Name.StartsWith("Test_"))
                .Select(t =>
                    {
                        if (typeof(IActionSource2).IsAssignableFrom(t))
                        {
                            return Create<IActionSource2>(t);
                        } 
                        else if (typeof(IActionSource).IsAssignableFrom(t))
                        {
                            var a = Create<IActionSource>(t);
                            if (a == null)
                            {
                                return null;
                            }
                            return a.ToIActionSource2();
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
                        return new ActionFilter(this.Context, parser);
                    })
                    .Where(x => x != null)
                    )

                .ToList();

            log.Info(actionSources.ListFormat());
            return actionSources;
        }
    }
}
