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
using System.Diagnostics;
using System.ComponentModel;

namespace hagen.plugin.google
{
    public class GoogleContacts : IActionSource
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string scope = "https://www.google.com/m8/feeds";
        readonly IContext context;
        Sidi.CredentialManagement.ICredentialProvider credentialProvider;

        public GoogleContacts(IContext context)
        {
            this.context = context;
            credentialProvider = Sidi.CredentialManagement.Factory.GetCredentialProvider(scope);
            credentialProvider.GetCredential();
        }

        static bool HasPrefix(string query, string prefix, out string subQuery)
        {
            var parts = Sidi.Util.Tokenizer.ToList(query);
            if (parts.Any() && Sidi.CommandLine.Parser.IsMatch(parts[0], prefix))
            {
                subQuery = parts.Skip(1).Join(" ");
                return true;
            }
            else
            {
                subQuery = null;
                return false;
            }
        }

        IEnumerable<IAction> IActionSource.GetActions(string query)
        {
            var contact = "Contact";

            if (!HasPrefix(query, contact, out query))
            {
                goto nothing;
            }

            if (!Regex.IsMatch(query, @"^[\s\w]{4,200}$"))
            {
                goto nothing;
            }

            var entries = ReadContacts(query);

            log.Info(entries.ListFormat());

            return entries.Select(e => (IAction)new ContactAction(this.context.LastExecutedStore, e));

            nothing:
                return Enumerable.Empty<IAction>();
        }

        public class ContactAction : ActionBase
        {
            [Browsable(true)]
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public Contact Contact
            {
                get; private set;
            }

            public ContactAction(ILastExecutedStore lastExecutedStore, Contact contact)
                : base(lastExecutedStore)
            {
                var name = String.Format("{0}: {1}", "Contact", contact.ToString());
                this.Name = name;
                this.Id = name;
                this.Contact = contact;
            }

            public override void Execute()
            {
                var id = Contact.Self.Split(new[] { '/' }).Last();
                var uri = String.Format("https://contacts.google.com/u/0/preview/{0}", id);
                Process.Start(uri);
            }
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
                    credentialProvider.GetCredential().UserName,
                    CancellationToken.None,
                    null);

            var parameters = new Google.GData.Client.OAuth2Parameters()
            {
                ClientId = secrets.ClientId,
                ClientSecret = secrets.ClientSecret,
                RedirectUri = redirectUri,
                Scope = scope,
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
