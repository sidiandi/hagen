// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            if (Control.ModifierKeys == Keys.Control)
            {
            }
            else
            {
                Action.Execute();
            }

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
