﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace hagen
{
    internal class AdapterIActionSource : IActionSource2
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AdapterIActionSource(IActionSource actionSource)
        {
            this.actionSource = actionSource;
        }

        public IActionSource actionSource { get; private set; }

        IEnumerable<IAction> SafeEnum(IEnumerable<IAction> data)
        {
            var e = data.GetEnumerator();
            for (; ; )
            {
                IAction x;
                try
                {
                    if (!e.MoveNext())
                    {
                        break;
                    }

                    x = e.Current;
                }
                catch (Exception ex)
                {
                    log.Warn(ex);
                    break;
                }

                yield return x;
            }
        }

        public IObservable<IAction> GetActions(string query)
        {
            return SafeEnum(actionSource.GetActions(query)).ToObservable(ThreadPoolScheduler.Instance);
        }
    }

    public static class IActionSourceExtensions
    {
        public static IActionSource2 ToIActionSource2(this IActionSource actionSource)
        {
            return new AdapterIActionSource(actionSource);
        }
    }
}
