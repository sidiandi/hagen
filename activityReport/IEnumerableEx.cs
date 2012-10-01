using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace activityReport
{
    public static class IEnumerableEx
    {
        public static IEnumerable<KeyValuePair<T, double>> SlidingSum<T>(this IEnumerable<T> e, Func<T, double> selector, int window)
        {
            double sum = 0;
            var sub = new double[window].Concat(e.Select(x => selector(x)));
            var eSub = sub.GetEnumerator();
            return e.Select(i =>
                {
                    sum += selector(i);
                    eSub.MoveNext();
                    sum -= eSub.Current;
                    return new KeyValuePair<T, double>(i, sum);
                });
        }
    }
}
