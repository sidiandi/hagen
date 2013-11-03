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

namespace hagen.ActionSource
{
    public class DatabaseLookup : IActionSource
    {
        public DatabaseLookup(Sidi.Persistence.Collection<Action> actions)
        {
            this.actions = actions;
        }

        Sidi.Persistence.Collection<Action> actions;

        IEnumerable<IAction> ToIActions(Action action)
        {
            yield return new ActionWrapper(action, actions);
        }
        
        public IEnumerable<IAction> GetActions(string query)
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
                cmd.CommandText = String.Format("select oid from {1} where Name like {0} order by LastUseTime desc", p.ParameterName, actions.Tablespie);
            }

            var r = actions.Query(cmd);
            return r.SelectMany(action => ToIActions(action)).ToList();
        }
    }
}
