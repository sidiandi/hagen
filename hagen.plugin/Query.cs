// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using System.Text.RegularExpressions;

namespace hagen
{
    public class Query : IQuery
    {
        List<string> tags;
        string text = String.Empty;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Query(IContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.Context = context;
        }

        const string tagPrefix = "#";

        static void SkipWhitespace(string s, ref int i)
        {
            i += Regex.Match(s.Substring(i), @"^\s*").Groups[0].Length;
        }

        static bool TryReadTag(string s, ref int i, out string tag)
        {
            if (!s.Substring(i).StartsWith(tagPrefix))
            {
                tag = null;
                return false;
            }
            var tagStart = i;
            i += tagPrefix.Length;

            var m = Regex.Match(s.Substring(i), @"^(\S+)");
            tag = s.Substring(tagStart, tagPrefix.Length + m.Groups[0].Length);
            i += m.Groups[0].Length;
            return true;
        }

        public static Query Parse(IContext context, string queryString)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            int i = 0; // current position in the query string
            var tags = new List<string>();
            for (;;)
            {
                SkipWhitespace(queryString, ref i);
                string tag;
                if (!TryReadTag(queryString, ref i, out tag))
                {
                    break;
                }
                tags.Add(tag);
            }
            if (tags.Count == 0)
            {
                i = 0;
            }
            else
            {
                if (i == queryString.Length)
                {
                    queryString = queryString + " ";
                    ++i;
                }
            }

            var text = queryString.Substring(i).OneLine(80);

            return new Query(context)
            {
                ParsedString = queryString,
                TextBegin = i,
                TextEnd = queryString.Length,
                tags = tags,
                text = text
            };
        }

        public string ParsedString
        {
            get; private set;
        }

        public int TextBegin
        {
            get; private set;
        }
        public int TextEnd
        {
            get; private set;
        }

        public string BuildQueryString()
        {
            return Tags.Concat(text).Join(" ");
        }

        public string Text { get { return text; } set { text = value; } } 

        public System.Collections.Generic.ICollection<string> Tags { get { return tags; } }

        IReadOnlyCollection<string> IQuery.Tags
        {
            get
            {
                return tags;
            }
        }

        public IContext Context { get; private set; }
    }
}
