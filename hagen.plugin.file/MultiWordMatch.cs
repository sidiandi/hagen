using Sidi.Util;
using System.Text.RegularExpressions;
using System.Linq;

namespace hagen
{
    internal class MultiWordMatch
    {
        private Regex[] terms;

        public MultiWordMatch(IQuery query)
        {
            terms = Tokenizer.ToArray(query.Text).Select(Extensions.SafeRegex)
                .Concat(query.Tags.Select(Extensions.EscapedRegex))
                .ToArray();
        }

        public MultiWordMatch(string query)
        {
            terms = Tokenizer.ToArray(query)
                .Select(Extensions.SafeRegex).ToArray();
        }

        public bool IsMatch(string text)
        {
            return terms.All(_ => _.IsMatch(text));
        }
    }
}