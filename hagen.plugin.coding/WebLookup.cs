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
using System.Text.RegularExpressions;

namespace hagen
{
    public class WebLookup : EnumerableActionSource
    {
        protected override IEnumerable<IResult> GetResults(IQuery queryObject)
        {
            var query = queryObject.Text.Trim();
            if (query.Length >= 3)
            {
                if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
                {
                }
                else
                {
                    yield return WebLookupAction("dev.azure.com Work Item", "https://dev.azure.com/CommonHostPlatform/chp/_search?text={0}*&type=workitem", query);
                    yield return WebLookupAction("Stackoverflow", "https://stackoverflow.com/search?q={0}", query);
                }
            }
        }

        IResult WebLookupAction(string title, string urlTemplate, string query)
        {
            var lastUsed = DateTime.MinValue;

            // try to parse query
            var p = Regex.Split(query, @"\s+");

            var priority = Priority.Low;

            if (p.Length >= 2 && title.StartsWith(p[0], StringComparison.InvariantCultureIgnoreCase))
            {
                priority = Priority.High;
                query = String.Join(" ", p.Skip(1));
            }

            var a = new ShellAction(
                String.Format(urlTemplate, System.Web.HttpUtility.UrlEncode(query)),
                String.Format("{0} \"{1}\"", title, query))
            {
                LastExecuted = lastUsed
            };

            return a.ToResult(priority);
        }
    }
}
