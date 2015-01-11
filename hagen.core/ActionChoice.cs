using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public class ActionChoice : IAction
    {
        public ActionChoice(string name, Func<IEnumerable<IAction>> actionProvider, System.Action<IList<IAction>> choose)
        {
            this.Name = name;
            this.actionProvider = actionProvider;
            this.choose = choose;
        }

        Func<IEnumerable<IAction>> actionProvider;
        System.Action<IList<IAction>> choose;

        public void Execute()
        {
            var actions = actionProvider().ToList();
            choose(actions);
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
