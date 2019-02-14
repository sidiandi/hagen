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
    class DatabaseLookup : IActionSource3
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

        IEnumerable<IResult> GetResults(IQuery query)
        {
            var terms = Tokenizer.ToList(query.Text.OneLine(80));

            var seen = new HashSet<string>();

            if (query.Tags.Any())
            {
                foreach (var i in GetResults(query.Tags, terms))
                {
                    i.Priority = Priority.High;
                    seen.Add(i.Action.Name);
                    yield return i;
                }
            }

            foreach (var i in GetResults(terms))
            {
                if (!seen.Contains(i.Action.Name))
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Selects results that match the terms
        /// </summary>
        /// <param name="terms"></param>
        /// <returns>List of results. Empty list if terms contain an SQL problem</returns>
        IEnumerable<IResult> GetResults(IEnumerable<string> terms)
        {
            if (!terms.Any(_ => _.Length >= 1))
            {
                return Enumerable.Empty<IResult>();
            }

            var cmd = actions.Connection.CreateCommand();

            var termsQuery = terms.Select((t, i) =>
            {
                var paramName = String.Format("@term{0}", i);
                var parameter = cmd.Parameters.Add(paramName, System.Data.DbType.String);
                parameter.Value = String.Format("%{0}%", t);
                return String.Format("Name like {0}", paramName);
            }).Join(" and ");

            cmd.CommandText = String.Format("select oid from {1} where {0} order by LastUseTime desc limit 50", termsQuery, actions.Table);

            IList<Action> r;
            try
            {
                r = actions.Query(cmd);
            }
            catch (System.Data.SQLite.SQLiteException e)
            {
                log.Info(e);
                return Enumerable.Empty<IResult>();
            }
            var results = r.SelectMany(action => ToIActions(action))
                .Select(a => a.ToResult(a.GetPriority(terms)))
                .ToList();
            return results;
        }

        IEnumerable<IResult> GetResults(IEnumerable<string> tags, IEnumerable<string> terms)
        {
            if (!tags.Any() && !terms.Any(_ => _.Length >= 1))
            {
                return Enumerable.Empty<IResult>();
            }

            var cmd = actions.Connection.CreateCommand();

            var termsQuery = terms.Select((t, i) =>
            {
                var paramName = String.Format("@term{0}", i);
                var parameter = cmd.Parameters.Add(paramName, System.Data.DbType.String);
                parameter.Value = String.Format("%{0}%", t);
                return String.Format("Name like {0}", paramName);
            }).Join(" and ");

            var tagsQuery = terms.Select((t, i) =>
            {
                var paramName = String.Format("@tag{0}", i);
                var parameter = cmd.Parameters.Add(paramName, System.Data.DbType.String);
                parameter.Value = String.Format("%{0}%", t);
                return String.Format("Name like {0}", paramName);
            }).Join(" and ");

            cmd.CommandText = String.Format($"select oid from {actions.Table} where ({tagsQuery}) and ({termsQuery}) order by LastUseTime desc limit 50");
            log.Info(cmd.CommandText);

            IList<Action> r;
            try
            {
                r = actions.Query(cmd);
            }
            catch (System.Data.SQLite.SQLiteException e)
            {
                log.Info(e);
                return Enumerable.Empty<IResult>();
            }
            var results = r.SelectMany(action => ToIActions(action))
                .Select(a => a.ToResult(a.GetPriority(terms)))
                .ToList();
            return results;
        }

        public bool IncludeInSearch;

        public IObservable<IResult> GetActions(IQuery query)
        {
            if (!IncludeInSearch)
            {
                return Observable.Empty<IResult>();
            }

            var results = GetResults(query);
            return results.ToObservable(ThreadPoolScheduler.Instance);
        }
    }
}
