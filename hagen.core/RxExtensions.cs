using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi
{
    public static class RxExtensions
    {
        public static IObservable<IObservable<DateTime>> TimeIntervals<T>(this IObservable<T> ticks)
        {
            return ticks.Select(_ => DateTime.Now).Window(2, 1);
        }
    }
}
