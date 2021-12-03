using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    public static class StringExtensions
    {
        public static bool ContainsIgnoreCase(this string text, string pattern)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, pattern, CompareOptions.IgnoreCase) >= 0;
        }

        public static bool FirstWordIs(this string text, string word, out string rest)
        {
            var firstWord = text.FirstWord();
            if (word.StartsWith(firstWord, StringComparison.OrdinalIgnoreCase))
            {
                var restBegin = FirstIndex(text, _ => char.IsWhiteSpace(_), firstWord.Length);
                rest = text.Substring(restBegin);
                return true;
            }
            else
            {
                rest = text;
                return false;
            }
        }

        public static int FirstIndex(this string s, Func<char, bool> f, int start = 0)
        {
            for (int i=start; i< s.Length;++i)
            {
                if (f(s[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string FirstWord(this string text)
        {
            var wordEnd = FirstIndex(text, _ => !char.IsWhiteSpace(_));
            if (wordEnd < 0) return String.Empty;
            return text.Substring(0, wordEnd);
        }
    }
}
