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
using mshtml;
using System.Web;
using Sidi.Util;
using System.IO;
using System.Text.RegularExpressions;
using SHDocVw;
using Sidi.Extensions;

namespace hagen.Plugin.Db
{
    static class mshtmlEx
    {
        public static bool IsInternetExplorer(this InternetExplorer ie)
        {
            return Path.GetFileNameWithoutExtension(ie.FullName).ToLower().Equals("iexplore");
        }

        public static string GetAttribute(this IHTMLElement element, string name)
        {
            var a = ((IHTMLElement4)element).getAttributeNode(name);
            if (a == null)
            {
                return String.Empty;
            }

            var r = a.nodeValue;
            if (r == null)
            {
                return String.Empty;
            }
            return r;
        }

        public static bool IsAttribute(this IHTMLElement element, string attributeName, string pattern)
        {
            return Regex.IsMatch(element.GetAttribute(attributeName), pattern, RegexOptions.IgnoreCase);
        }

        public static IEnumerable<IHTMLElement> GetChildren(this IHTMLElement e)
        {
            return ((IHTMLElementCollection)e.children)
                .Cast<IHTMLElement>();
        }

        public static IEnumerable<IHTMLElement> GetAllChildren(this IHTMLElement e)
        {
            return ((IHTMLElementCollection)e.all)
                .Cast<IHTMLElement>();
        }

        public static IEnumerable<IHTMLElement> GetChildren(this IHTMLElement e, string tagName)
        {
            return e.GetChildren()
                .Where(i => i.tagName.Equals(tagName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string GetInnerText(this IHTMLElement e)
        {
            var r = e.innerText;
            if (r == null)
            {
                r = String.Empty;
            }
            return HttpUtility.HtmlDecode(r);
        }

        public static string[][] ToText(this IHTMLElement[][] table)
        {
            return table.Select(r => r.Select(td => td.GetInnerText()).ToArray()).ToArray();
        }

        public static IHTMLElement[][] ParseTable(this IHTMLElement table)
        {
            return table.GetChildren("TBODY")
                .SelectMany(tbody => tbody.GetChildren("TR")
                    .Select(tr =>
                    {
                        return tr.GetChildren("TD")
                            .ToArray();
                    }))
                    .Skip(1)
                    .ToArray();
        }

        public static string Dump(this string[][] table)
        {
            var w = new StringWriter();
            foreach (var r in table.Counted())
            {
                w.WriteLine("{0}: {1}", r.Key, r.Value.Join("|"));
            }
            return w.ToString();
        }

        public static IEnumerable<IHTMLElement> GetElementsByTagName(this IHTMLDocument3 document, string tagName)
        {
            return document.getElementsByTagName(tagName).Cast<IHTMLElement>();
        }

        public static bool IsTag(this IHTMLElement e, string tagName)
        {
            return e.tagName.Equals(tagName, StringComparison.InvariantCultureIgnoreCase);
        }

        public static IHTMLElement GetAncestor(this IHTMLElement e, string tagName)
        {
            for (e = e.parentElement; e != null; e = e.parentElement)
            {
                if (e.IsTag(tagName))
                {
                    return e;
                }
            }
            return e;
        }

        public static IHTMLElement GetElementByName(this IHTMLDocument3 document, string name)
        {
            try
            {
                return document.getElementsByName(name).Cast<IHTMLElement>().First();
            }
            catch (InvalidOperationException e)
            {
                throw new Exception(document.documentElement
                    .GetAllChildren()
                    .Select(n => String.Format("{0} {1}", n.tagName, n.GetAttribute("name"))).Join(), e);
            }
        }

        public static IHTMLDOMNode GetChild(this IHTMLDOMNode root, Func<IHTMLDOMNode, bool> p)
        {
            if (p(root))
            {
                return root;
            }

            foreach (IHTMLDOMNode c in root.childNodes)
            {
                var r = GetChild(c, p);
                if (r != null)
                {
                    return r;
                }
            }

            return null;
        }

        public static IHTMLDocument3 GetFrame(this IHTMLDocument3 document, string frameId)
        {
            dynamic d = document;
            var txnWindow = ((IHTMLWindow2)d.frames[frameId]);
            return (IHTMLDocument3)txnWindow.document;
        }

    }
}
