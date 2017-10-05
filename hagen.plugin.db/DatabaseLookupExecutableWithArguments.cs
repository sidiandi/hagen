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
using Sidi.IO;
using hagen;
using Sidi.Util;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Sidi.Extensions;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;

namespace hagen.Plugin.Db
{
    class DatabaseLookupExecutableWithArguments : IActionSource2
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DatabaseLookupExecutableWithArguments(Sidi.Persistence.Collection<Action> actions)
        {
            this.actions = actions;
        }

        Sidi.Persistence.Collection<Action> actions;

        class ActionWrapper : IAction
        {
            public ActionWrapper(hagen.Plugin.Db.Action action, string arguments)
            {
                this.Action = action;
                if (Action.CommandObject is StartProcess)
                {
                    var sp = (StartProcess)Action.CommandObject;
                    sp.Arguments = Arguments;
                }

                this.Arguments = arguments;
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

            public string Arguments
            {
                set; get;
            }

            public void Execute()
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                }
                else
                {
                    this.Action.Execute();
                }
            }

            public string Name
            {
                get
                {
                    return String.Format("{0} {1} ({2})", Action.Name, this.Arguments, Action.CommandDetails);
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

        IEnumerable<IAction> ToIActions(Action action, string arguments)
        {
            yield return new ActionWrapper(action, arguments);
        }

        IEnumerable<IAction> GetActionsEnum(string fullQuery)
        {
            var m = Regex.Match(fullQuery, @"^(?<query>\S+)\s+(?<arguments>.+)");
            if (!m.Success)
            {
                return Enumerable.Empty<IAction>();
            }

            var query = m.Groups["query"].Value;
            var arguments = m.Groups["arguments"].Value;

            using (new LogScope(log.Info, "query: {0}", query))
            {
                var cmd = actions.Connection.CreateCommand();

                var p = cmd.CreateParameter();
                p.ParameterName = "$p";
                p.DbType = System.Data.DbType.String;
                p.Value = "%" + query.Truncate(0x100) + "%";
                cmd.Parameters.Add(p);

                if (String.IsNullOrEmpty(query) || query.Length <= 2)
                {
                    cmd.CommandText = String.Format("select oid from {1} where Name like {0} order by LastUseTime desc limit 20", p.ParameterName, actions.Table);
                }
                else
                {
                    cmd.CommandText = String.Format("select oid from {1} where Name like {0} order by LastUseTime desc", p.ParameterName, actions.Table);
                }

                var r = actions.Query(cmd);
                return r.SelectMany(action => ToIActions(action, arguments)).ToList();
            }
        }

        public bool IncludeInSearch;

        public IObservable<IAction> GetActions(string query)
        {
            if (!IncludeInSearch)
            {
                return Observable.Empty<IAction>();
            }

            var actions = GetActionsEnum(query);
            actions = Filters.OpenInVlc(actions);

            return actions.ToObservable(ThreadPoolScheduler.Instance);
        }
    }
}
