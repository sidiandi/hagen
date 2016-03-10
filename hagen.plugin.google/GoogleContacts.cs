using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Client;
using Google.Contacts;
using Sidi.Extensions;
using Sidi.Test;
using System.IO;
using Sidi.IO;
using System.Threading;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2;

namespace hagen.plugin.google
{
    public class GoogleContacts : IActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly IContext context;

        public GoogleContacts(IContext context)
        {
            this.context = context;

            /*
            var secrets = Paths.BinDir.CatDir("client_secret_292564741141-6fa0tqv21ro1v8s28gj4upei0muvuidm.apps.googleusercontent.com.json").Read(GoogleClientSecrets.Load).Secrets;

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    new string[] { "https://www.google.com/m8/feeds" },
                    "andreas.grimme@gmx.net",
                    CancellationToken.None,
                    new FileDataStore("Contacts3")).Result;

            oAuth2Parameters = new OAuth2Parameters()
            {
                ClientId = secrets.ClientId,
                ClientSecret = secrets.ClientSecret,
                RedirectUri = redirectUri,
                Scope = "https://www.google.com/m8/feeds",
                AccessToken = credential.Token.AccessToken,
                RefreshToken = credential.Token.RefreshToken,
            };
            */
        }

        OAuth2Parameters oAuth2Parameters;

        IEnumerable<IAction> IActionSource.GetActions(string query)
        {
            if (!Regex.IsMatch(query, @"^[\s\w]{4,200}$"))
            {
                return Enumerable.Empty<IAction>();
            }

            var entries = ReadContacts(query);

            log.Info(entries.ListFormat());

            return entries.Select(e =>
            {
                return (IAction)new SimpleAction(this.context.LastExecutedStore, e.ToString(), e.ToString(), () =>
                {
                });

            });
        }

        IList<Contact> ReadContacts(string query)
        {
            return ReadContactsAsync(query).Result;
        }

        // Installed (non-web) application
        private static string redirectUri = "urn:ietf:wg:oauth:2.0:oob";

        public async Task<IList<Contact>> ReadContactsAsync(string query)
        {
            var secrets = Paths.BinDir.CatDir("client_secret_292564741141-6fa0tqv21ro1v8s28gj4upei0muvuidm.apps.googleusercontent.com.json").Read(GoogleClientSecrets.Load).Secrets;

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    new string[] { "https://www.google.com/m8/feeds" },
                    "andreas.grimme@gmx.net",
                    CancellationToken.None,
                    null);

            var parameters = new Google.GData.Client.OAuth2Parameters()
            {
                ClientId = secrets.ClientId,
                ClientSecret = secrets.ClientSecret,
                RedirectUri = redirectUri,
                Scope = "https://www.google.com/m8/feeds",
                AccessToken = credential.Token.AccessToken,
                RefreshToken = credential.Token.RefreshToken,
            };

            var contacts = new ContactsRequest(new RequestSettings("hagen", parameters));
            var q = new FeedQuery("https://www.google.com/m8/feeds/contacts/default/full")
            {
                Query = query
            };
            var feed = contacts.Get<Contact>(q);
            var entries = feed.Entries.ToList();
            return entries;
        }
    }
}
