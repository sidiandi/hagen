using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen
{
    public interface IActionSource
    {
        IList<IAction> GetActions(string query);
    }
}
