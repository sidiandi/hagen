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

namespace hagen
{
    public class SimpleAction : IAction, ISecondaryActions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        System.Action action;
        readonly string name;
        readonly string id;
        readonly ILastExecutedStore lastExecutedStore;

        public SimpleAction(ILastExecutedStore lastExecutedStore, string id, string name, System.Action action)
        {
            this.id = id;
            this.name = name;
            this.action = action;
            this.lastExecutedStore = lastExecutedStore;
        }

        public SimpleAction(string id, string name, System.Action action)
        {
            this.id = id;
            this.name = name;
            this.action = action;
            this.lastExecutedStore = null;
        }

        public void Execute()
        {
            try
            {
                if (lastExecutedStore != null)
                {
                    lastExecutedStore.Set(id);
                }
                action();

            }
            catch (Exception ex)
            {
                log.Error(this.Name, ex);
            }
        }

        public IEnumerable<IAction> GetActions()
        {
            return SecondaryActions;
        }

        public IList<IAction> SecondaryActions { get; private set; } = new List<IAction>(); 

        public string Name
        {
            get { return name; }
        }

        public override string ToString() => Name;

        public System.Drawing.Icon Icon
        {
            get; set; 
        }

        public string Id
        {
            get
            {
                return id;
            }
        }

        public DateTime LastExecuted
        {
            get
            {
                return lastExecutedStore == null
                    ? DateTime.MinValue
                    : lastExecutedStore.Get(id);
            }
        }
    }
}
