using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hagen.ActionSource
{
    public class WebLookup : IActionSource
    {
        public IList<IAction> GetActions(string query)
        {
            var webLookup = new List<IAction>();
            webLookup.Add(WebLookupAction("Google", "http://www.google.com/search?q={0}", query));
            webLookup.Add(WebLookupAction("Wikipedia", "http://en.wikipedia.org/wiki/Special:Search?search={0}&go=Go", query));
            webLookup.Add(WebLookupAction("Leo", "http://dict.leo.org/?lp=ende&search={0}", query));
            return webLookup;
        }

        IAction WebLookupAction(string title, string urlTemplate, string query)
        {
            return new ShellAction(
                String.Format(urlTemplate, System.Web.HttpUtility.UrlEncode(query)),
                String.Format("{0} \"{1}\"", title, query));
        }
    }
}