using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen
{
    public class SimpleAction : IAction
    {
        System.Action action;
        string name;

        public SimpleAction(string name, System.Action action)
        {
            this.name = name;
            this.action = action;
        }

        public void Execute()
        {
            action();
        }

        public string Name
        {
            get { return name; }
        }

        public System.Drawing.Icon Icon
        {
            get { return null; }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
