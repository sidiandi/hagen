using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;

namespace hagen
{
    public static class PathListExtension
    {
        public static string JoinTruncated<T>(this IEnumerable<T> list, string sep, int maxLength)
        {
            var sb = new StringBuilder();
            var e = list.GetEnumerator();
            if (e.MoveNext())
            {
                for (; ; )
                {
                    sb.Append(e.Current.ToString());
                    if (e.MoveNext())
                    {
                        sb.Append(sep);
                        if (sb.Length > maxLength)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            int i = 0;
            for (; ; ++i)
            {
                if (!e.MoveNext())
                {
                    break;
                }
            }
            if (i > 0)
            {
                sb.AppendFormat("and {0} more", i);
            }
            return sb.ToString();
        }
    }
}
