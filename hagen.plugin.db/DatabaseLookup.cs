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
using System.Windows.Forms;
using Sidi.Forms;

namespace hagen.Plugin.Db
{
    class DatabaseLookup : EnumerableActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly Sidi.Persistence.Collection<Action> actions;
        public bool IncludeInSearch;

        public DatabaseLookup(Sidi.Persistence.Collection<Action> actions)
        {
            this.actions = actions;
        }

        IEnumerable<IAction> ToIActions(Action action)
        {
            yield return new ActionWrapper(action, actions);
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            var terms = query.GetTerms();
            var tags = query.Tags;

            if (!tags.Any() && !terms.Any(_ => _.Length >= 1))
            {
                return Enumerable.Empty<IResult>();
            }

            var cmd = actions.Connection.CreateCommand();

            var termsQuery = And(terms.Select((t, i) =>
            {
                var paramName = String.Format("@term{0}", i);
                var parameter = cmd.Parameters.Add(paramName, System.Data.DbType.String);
                parameter.Value = String.Format("%{0}%", t);
                return String.Format("Name like {0}", paramName);
            }));

            var tagsQuery = And(tags.Select((t, i) =>
            {
                var paramName = String.Format("@tag{0}", i);
                var parameter = cmd.Parameters.Add(paramName, System.Data.DbType.String);
                parameter.Value = String.Format("%{0}%", t);
                return String.Format("Name like {0}", paramName);
            }));

            cmd.CommandText = String.Format($"select oid from {actions.Table} where {And(tagsQuery, termsQuery)} order by LastUseTime desc limit 50");
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
                .Select(a => a.ToResult(query))
                .ToList();

            if (results.Count == 0)
            {
                var markdownLink = MarkdownLink.Parse(query.RawText);
                if (markdownLink != null)
                {
                    results.Add(new SimpleAction("add", $"Add {markdownLink.Title}", () =>
                    {
                        var factory = new FileActionFactory();
                        var title = Prompt.GetText("Title");
                        var action = factory.FromUrl(markdownLink.Href, markdownLink.Title);
                        actions.Add(action);
                    }).ToResult());
                }

                var namedUrl = NamedUrl.Parse(query.RawText);
                if (namedUrl != null)
                {
                    results.Add(new SimpleAction("add", $"Add {namedUrl.Title}", () =>
                    {
                        var factory = new FileActionFactory();
                        var title = Prompt.GetText("Title");
                        var action = factory.FromUrl(namedUrl.Url, namedUrl.Title);
                        actions.Add(action);
                    }).ToResult());
                }
            }
            return results;
        }

        static string And(params string[] booleanClause)
        {
            return And((IEnumerable<string>)booleanClause);
        }

        static string And(IEnumerable<string> booleanClause)
        {
            return booleanClause.Where(_ => !String.IsNullOrEmpty(_)).Select(_ => $"({_})").Join(" and ");
        }

    }
}
