using Amg.Build;
using Amg.Extensions;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amg.Util
{
    public class PersonInfo : IPersonInfo
    {
        public static PersonInfo Create() => Once.Create<PersonInfo>();

        protected PersonInfo()
        {
        }

        static IDictionary<string, object[]> ToDictionary(SearchResult sr)
        {
            var data = sr.Properties.Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(_ => (string)_.Key, _ => ((ResultPropertyValueCollection)_.Value).Cast<object>().ToArray());
            return data;
        }

        [Once]
        public virtual async Task<Person> GetCurrent()
        {
            var gid = System.Environment.UserName;
            var sr = await FindAccountByGid(gid);
            return GetUser(sr);
        }

        static Person GetUser(SearchResultCollection sr)
        {
            if (sr is null || sr.Count < 1) return null;
            return ToPerson(sr[0]);
        }

        internal static Person ToPerson(SearchResult sr)
        {
            var person = new Person();
            var searchResultDictionary = ToDictionary(sr);
            foreach (var i in searchResultDictionary)
            {
                person.Properties[i.Key] = i.Value.Select(_ => _.SafeToString()).ToArray();
            }
            return person;
        }

        [Once]
        public virtual async Task<Person> GetByMail(string mail)
        {
            if (mail is null)
            {
                return null;
            }

            var sr = await FindAccountByEmail(mail);
            return GetUser(sr);
        }

        static Task<SearchResultCollection> FindAccountByEmail(string email) => Task.Factory.StartNew(() =>
        {
            string filter = string.Format("(mail={0})", email);

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

                            return result;
                        }
                    }
                }
            }
            return null;
        });

        static Task<SearchResultCollection> FindAccountByGid(string gid) => Task.Factory.StartNew(() =>
        {
            string filter = string.Format("(samaccountname={0})", gid);

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

                            return result;
                        }
                    }
                }
            }
            return null;
        });
    }
}
