using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen
{
    public class Filter : IActionSource3
    {
        public Filter(IActionSource3 source, Func<IObservable<IResult>, IObservable<IResult>> filter)
        {
            this.source = source;
            this.filter = filter;
        }

        readonly IActionSource3 source;
        readonly Func<IObservable<IResult>, IObservable<IResult>> filter;
    
        public IObservable<IResult> GetActions(IQuery query)
        {
            return filter(source.GetActions(query));
        }
    }
}
