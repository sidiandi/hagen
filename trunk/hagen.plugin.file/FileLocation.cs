using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen.ActionSource
{
    public class OpenFile : IActionSource
    {
        public IList<IAction> GetActions(string query)
        {
            var fl = FileLocation.Parse(query);
            return new IAction[] { new ShellAction(fl.Path) }.ToList();
        }
    }
}
