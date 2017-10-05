// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace hagen
{
    static class EnumerableExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IEnumerable<TResult> SafeSelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return source.SelectMany(_ =>
            {
                try
                {
                    return selector(_);
                }
                catch
                {
                    return Enumerable.Empty<TResult>();
                }
            });
        }

        public static IEnumerable<T> UntilNull<T>(Func<T> provider)
        {
            for (;;)
            {
                var v = provider();
                if (v == null)
                {
                    break;
                }
                yield return v;
            }
        }
    }
}
