using NUnit.Framework;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using Sidi.IO;
using Google.GData.Client;
using Google.Contacts;
using Sidi.Extensions;
using Sidi.Test;

namespace hagen.plugin.google.Tests
{
    [TestFixture]
    public class GoogleContactsTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit("does not run on Jenkins")]
        public void ReadContacts()
        {
            ReadContactsAsync().Wait();
        }

        // Installed (non-web) application
        private static string redirectUri = "urn:ietf:wg:oauth:2.0:oob";

        public async Task ReadContactsAsync()
        {
            var secrets = Paths.BinDir.CatDir("client_secret_292564741141-6fa0tqv21ro1v8s28gj4upei0muvuidm.apps.googleusercontent.com.json").Read(GoogleClientSecrets.Load).Secrets;

            var credentialProvider = Sidi.CredentialManagement.Factory.GetCredentialProvider("https://www.google.com/m8/feeds");

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                new string[] { "https://www.google.com/m8/feeds" },
                credentialProvider.GetCredential().UserName,
                CancellationToken.None,
                new FileDataStore("Contacts2"));

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
                Query = "Grimme"
            };
            var feed = contacts.Get<Contact>(q);

            log.Info(feed.Entries.ListFormat());
        }
    }
}