using Amg.Util;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hagen.plugin.healthineers
{
    public class Scd : EnumerableActionSource
    {
        public static bool IsMail(string mail) => Regex.IsMatch(mail, @"\w+\@\w+");
        public static IEnumerable<string> FindMails(string mail)
            => Regex.Matches(mail, @"[\w.]+\@([a-z-]+\.)+[a-z]+")
                .Cast<Match>()
                .Select(_ => _.Groups[0].Value)
                .ToList();

        public IEnumerable<Person> FindPersonsByFilter(string filter)
        {
            using (DirectoryEntry gc = new DirectoryEntry("LDAP:"))
            {
                foreach (DirectoryEntry z in gc.Children)
                {
                    using (DirectoryEntry root = z)
                    {
                        using (DirectorySearcher searcher = new DirectorySearcher(root, filter))
                        {
                            searcher.ReferralChasing = ReferralChasingOption.All;
                            SearchResultCollection result = searcher.FindAll();

                            return result.Cast<SearchResult>()
                                .Select(PersonInfo.ToPerson)
                                .ToList();
                        }
                    }
                }
            }
            return Enumerable.Empty<Person>();
        }

        public IEnumerable<Person> FindPersons(string searchTerm)
        {
            var tokens = Sidi.Util.Tokenizer.ToArray(searchTerm);

            var persons = Enumerable.Empty<Person>();

            if (tokens.Length == 2 && tokens.All(_ => _.Length >= 3))
            {
                persons = persons.Concat(FindPersonsByFilter(
                    $"(& (givenname={tokens[0]}*)(sn={tokens[1]}*))"));
            }
            
            foreach (var m in FindMails(searchTerm))
            {
                var filter = $"(mail={m})";
                persons = persons.Concat(FindPersonsByFilter(filter));
            }

            return persons;
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            return FindPersons(query.Text)
                    .Select(_ => new SimpleAction(_.Mail, 
                        $"{_.DisplayName} ({_.Department})", () => { }))
                    .Select(_ => _.ToResult())
                    .ToList();
        }
    }
}
