using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    }
}
