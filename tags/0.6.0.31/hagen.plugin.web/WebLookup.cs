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

namespace hagen.ActionSource
{
    public class WebLookup : IActionSource
    {
        public IEnumerable<IAction> GetActions(string query)
        {
            query = query.Trim();
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                yield return new ShellAction(query, String.Format("Open URL {0}", query));
            }
            else
            {
                yield return WebLookupAction("Google", "http://www.google.com/search?q={0}", query);
                yield return WebLookupAction("Wikipedia", "http://en.wikipedia.org/wiki/Special:Search?search={0}&go=Go", query);
                yield return WebLookupAction("Translate", "http://translate.google.com/#auto/en/{0}", query);
                yield return WebLookupAction("Leo", "http://dict.leo.org/?lp=ende&search={0}", query);
                yield return WebLookupAction("Preis", "http://www.heise.de/preisvergleich/?fs={0}&x=0&y=0&in=", query);
                yield return WebLookupAction("Contacts", "https://www.google.com/contacts/#contacts/search/{0}", query);
            }
        }

        IAction WebLookupAction(string title, string urlTemplate, string query)
        {
            return new ShellAction(
                String.Format(urlTemplate, System.Web.HttpUtility.UrlEncode(query)),
                String.Format("{0} \"{1}\"", title, query));
        }
    }
}
