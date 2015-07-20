using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen
{
    public interface IActionSource2
    {
        IObservable<IAction> GetActions(string query);
    }
}
