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

using Amg.Extensions;
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
            var iconProvider = queryObject.Context.GetService<IFileIconProvider>();
            if (query.Length >= 3)
            {
                if (!Uri.IsWellFormedUriString(query, UriKind.Absolute))
                {
                    return new[]{
                        AzureDevopsSearch(iconProvider, "CommonHostPlatform", query),
                        AzureDevopsSearch(iconProvider, "shs-baukasten", query),
                        WebLookupAction(
                            iconProvider,
                            "Workitem ID",
                            "https://helios.healthcare.siemens.com/tfs/Projects/Numaris/_workitems/edit/{0}",
                            query,
                            queryFilter: query => Regex.IsMatch(query, @"^\d{4,8}$")
                            ),
                        WebLookupAction(iconProvider, "Helios", "https://helios.healthcare.siemens.com/tfs/Projects/_search?text={0}", query),
                        WebLookupAction(iconProvider, "Stackoverflow", "https://stackoverflow.com/search?q={0}", query),
                        WebLookupAction(iconProvider, "Microsoft Docs", "https://docs.microsoft.com/en-US/search/?search={0}", query),
                        WebLookupAction(iconProvider, "ChpWeb", "https://chp.healthineers.siemens.com/?q={0}", query),
                        WebLookupAction(iconProvider, "SOC", "https://soc.siemens.cloud/search/user?searchterm={0}", query),
                        WebLookupAction(iconProvider, "SCD", "https://scd.siemens.com/luz/IdentitySearch?cn={0}&maxanz=50&suchart=schnell&utI=I&utX=X&utT=T&rtH=H&rtS=S&rtZ=Z&rtO=O&rtAktiv=A", query),
                        WebLookupAction(iconProvider, "LinkedIn", "https://www.linkedin.com/search/results/all/?keywords={0}", query)
                        }.NotNull()
                        .ToList();
                }
            }

            return Enumerable.Empty<IResult>();
        }

        IResult AzureDevopsSearch(IFileIconProvider iconProvider, string organization, string query)
            => WebLookupAction(iconProvider, organization + " Azure Devops", $"https://dev.azure.com/{organization}/_search?text={{0}}*&type=wiki", query);

        IResult? WebLookupAction(
            IFileIconProvider iconProvider,
            string title,
            string urlTemplate,
            string query,
            Func<string, bool>? queryFilter = null
            )
        {
            var lastUsed = DateTime.MinValue;

            // try to parse query
            var p = Regex.Split(query, @"\s+");

            var priority = Priority.Low;

            if (p.Length >= 2 && title.StartsWith(p[0], StringComparison.InvariantCultureIgnoreCase))
            {
                priority = Priority.High;
                query = String.Join(" ", p.Skip(1)).Trim();
            }

            if (queryFilter is { })
            {
                if (queryFilter(query))
                {
                    priority = Priority.High;
                }
                else
                {
                    return null;
                }
            }

            var a = new ShellAction(
                iconProvider,
                String.Format(urlTemplate, System.Web.HttpUtility.UrlEncode(query)),
                String.Format("{0} \"{1}\"", title, query))
            {
                LastExecuted = lastUsed
            };

            return a.ToResult(priority);
        }
    }
}
