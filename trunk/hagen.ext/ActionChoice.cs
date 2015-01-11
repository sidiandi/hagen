using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public class ActionChoice : IAction
    {
        public ActionChoice(string name, Func<IEnumerable<IAction>> actionProvider)
        {
            this.Name = name;
            this.actionProvider = actionProvider;
        }

        Func<IEnumerable<IAction>> actionProvider;

        public void Execute()
        {
            var actions = actionProvider().ToList();
        }

        public string Name
        {
            get;
            private set;
        }

        public System.Drawing.Icon Icon
        {
            get { return null; }
        }
    }
}
