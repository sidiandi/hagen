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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace hagen.Plugin.Db
{   
    public class ActionWrapper : IAction, IStorable
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

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Action Action { set; get; }

        Sidi.Persistence.Collection<Action> Data;
        
        public void Execute()
        {
            if (Control.ModifierKeys == Keys.Control)
            {
            }
            else
            {
                Action.Execute();
            }

            Data.Update(Action);
        }

        public void Store()
        {
            Data.Update(this.Action);
        }

        public string Name
        {
            get
            {
                return String.Format("{0} ({1}) {2}", Action.Name, Action.CommandDetails, Data.Connection.DataSource);
            }
        }

        [System.ComponentModel.Browsable(false)]
        public System.Drawing.Icon Icon
        {
            get { return Action.Icon; }
        }

        public string Id
        {
            get
            {
                var ia = (IAction)Action;
                return ia.Id;
            }
        }

        public DateTime LastExecuted
        {
            get
            {
                return this.Action.LastExecuted;
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
