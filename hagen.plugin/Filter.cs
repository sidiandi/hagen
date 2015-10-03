using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen
{
    public class Filter : IActionSource
    {
        public Filter(IActionSource source, Func<IEnumerable<IAction>, IEnumerable<IAction>> filter)
        {
            this.source = source;
            this.filter = filter;
        }

        IActionSource source;
        Func<IEnumerable<IAction>, IEnumerable<IAction>> filter;
    
        public IEnumerable<IAction>  GetActions(string query)
        {
 	        return filter(source.GetActions(query));
        }
    }
}
