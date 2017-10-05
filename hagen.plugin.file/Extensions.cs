using System;
using System.Text.RegularExpressions;

namespace hagen
{
    static internal class Extensions
    {
        public static Regex SafeRegex(string pattern)
        {
            try
            {
                return new Regex(pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            }
            catch (System.ArgumentException)
            {
            }

            return new Regex(Regex.Escape(pattern), RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
        }

    }
}