using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public static class Extension
    {
        public static IEnumerable<T> Create<T>(this IEnumerable<Type> types)
        {
            return types
                .Where(t => typeof(T).IsAssignableFrom(t) && !object.Equals(typeof(T), t))
                .Select(t =>
                {
                    var defaultConstructor = t.GetConstructor(new Type[] { });
                    if (defaultConstructor == null)
                    {
                        return default(T);
                    }

                    return (T)defaultConstructor.Invoke(new object[] { });
                })
                .Where(x => x != null);
        }
    }
}
