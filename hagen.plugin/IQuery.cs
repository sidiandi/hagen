using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public interface IQuery
    {
        string Text { get; }
        System.Collections.Generic.IReadOnlyCollection<string> Tags { get; }
    }
}
