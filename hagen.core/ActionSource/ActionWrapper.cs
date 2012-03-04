using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen.ActionSource
{
    public class ActionWrapper : IAction
    {
        public ActionWrapper(Action action, Sidi.Persistence.Collection<Action> data)
        {
            this.Data = data;
            this.Action = action;
            action.PropertyChanged += (s, e) =>
                {
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, e);
                    }
                };
        }

        public Action Action;
        public Sidi.Persistence.Collection<Action> Data;
        
        public void Execute()
        {
            Action.Execute();
            Action.LastUseTime = DateTime.Now;
            Data.Update(Action);
        }

        public string Name
        {
            get
            {
                return String.Format("{0} ({1})", Action.Name, Action.CommandDetails);
            }
        }

        public System.Drawing.Icon Icon
        {
            get { return Action.Icon; }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
