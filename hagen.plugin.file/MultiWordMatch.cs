using Sidi.Util;
using System.Text.RegularExpressions;
using System.Linq;

namespace hagen
{
    internal class MultiWordMatch
    {
        private Regex[] terms;

        public MultiWordMatch(string query)
        {
            terms = Tokenizer.ToArray(query).Select(Extensions.SafeRegex).ToArray();
        }

        public bool IsMatch(string text)
        {
            return terms.All(_ => _.IsMatch(text));
        }
    }
}