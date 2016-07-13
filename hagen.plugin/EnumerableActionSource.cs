// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace hagen
{
    public abstract class EnumerableActionSource : IActionSource3
    {
        protected abstract IEnumerable<IResult> GetResults(IQuery query);

        public IObservable<IResult> GetActions(IQuery query)
        {
            return GetResults(query).ToObservable(ThreadPoolScheduler.Instance);
        }
    }
}
