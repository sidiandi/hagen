using Google.GData.Client;
using hagen;
using Sidi.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace hagen
{
    public class GoogleCredentials
    {
        [Usage("Email Adress"), Persistent]
        public string Email { get; set; }

        [Usage("Google password"), Persistent, Password]
        public string Password;
    }

    public class Contacts : IActionSource2
    {
        public IObservable<IAction> GetActions(string query)
        {
            return GetActionsEnum(query).ToObservable(Scheduler.NewThread);
        }

        IEnumerable<IAction> GetActionsEnum(string query)
        {
            bool credentialsValid = true;

            while (true)
            {
                var rs = new new RequestSettings()
                var r = new Google.Contacts.ContactsRequest(("hagen", c.Email, c.Password));
                    var feed = r.GetContacts(query);
                    return feed.Entries
                        .Select(x => (IAction)new SimpleAction(x.Name.ToString(), () => { }))
                        .ToList();
                }
                catch (Google.GData.Client.InvalidCredentialsException ex)
                {
                    credentialsValid = false;
                }
            }
        }
    }
}
