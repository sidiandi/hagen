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

namespace hagen.Plugin.Db
{
    public class DatabaseLookup : IActionSource3
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DatabaseLookup(Sidi.Persistence.Collection<Action> actions)
        {
            this.actions = actions;
        }

        Sidi.Persistence.Collection<Action> actions;

        IEnumerable<IAction> ToIActions(Action action)
        {
            yield return new ActionWrapper(action, actions);
        }

        IEnumerable<IResult> GetActionsEnum(IQuery query)
        {
            var terms = Tokenizer.ToList(query.Text.OneLine(80));

            foreach (var i in GetResults(terms.Concat(query.Tags)))
            {
                i.Priority = Priority.High;
                yield return i;
            }

            foreach (var i in GetResults(terms))
            {
                i.Priority = Priority.Low;
                yield return i;
            }
        }

        IEnumerable<IResult> GetResults(IEnumerable<string> terms)
        {
            if (!terms.Any())
            {
                return Enumerable.Empty<IResult>();
            }

            var termsQuery = terms.Select(t => String.Format("Name like {0}", ("%" + t + "%").Quote())).Join(" and ");

            var cmd = actions.Connection.CreateCommand();

            if (terms.Sum(t => t.Length) <= 2)
            {
                cmd.CommandText = String.Format("select oid from {1} where {0} order by LastUseTime desc limit 20", termsQuery, actions.Table);
            }
            else
            {
                cmd.CommandText = String.Format("select oid from {1} where {0} order by LastUseTime desc limit 20", termsQuery, actions.Table);
            }

            var r = actions.Query(cmd);
            var results = r.SelectMany(action => ToIActions(action)).Select(a => a.ToResult(Priority.High)).ToList();
            return results;
        }

        public bool IncludeInSearch;

        public IObservable<IResult> GetActions(IQuery query)
        {
            if (!IncludeInSearch)
            {
                return Observable.Empty<IResult>();
            }

            var results = GetActionsEnum(query);
            return results.ToObservable(ThreadPoolScheduler.Instance);
        }
    }
}
