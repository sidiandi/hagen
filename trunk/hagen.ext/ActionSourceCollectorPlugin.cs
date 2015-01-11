using Sidi.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public class ActionSourceCollector : IPlugin
    {
        IContext Context { get; set; }

        public virtual void Init(IContext context)
        {
            Context = context;
        }

        public virtual IEnumerable<IActionSource2> GetActionSources()
        {
            var types = GetType().Assembly.GetTypes();

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
                        var contextCtor = t.GetConstructor(new Type[]{ typeof(IContext) } );
                        if (contextCtor != null)
                        {
                            plugin = contextCtor.Invoke(new object[] { this.Context });
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
                        return new ActionFilter(this.Context, parser);
                    })
                    .Where(x => x != null)
                    )

                .ToList();
        }
    }
}
