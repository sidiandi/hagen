using Amg.Extensions;
using Azure.Core;
using Microsoft.Graph;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;
using Sidi.Forms;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagen
{
    public class People : EnumerableActionSource
    {
        readonly Task<GraphServiceClient> client;

        public People()
        {
            client = ProvideClient();
        }

        Task<User> Me;

        async Task<GraphServiceClient> ProvideClient()
        {
            var token = await GraphUtil.GetTokenFromGraphExplorer();
            var client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                        await Task.CompletedTask;
                    }));
            Me = client.Me.Request().GetAsync();
            return client;
        }

        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            if (!client.IsCompleted || query.Text.Length < 4)
            {
                return Enumerable.Empty<IResult>();
            }

            var options = new[]
            {
                new QueryOption("$search", query.Text)
            };
            var page = client.Result.Me.People.Request(options).GetAsync().Result;

            return page.Select(i =>
            {
                var person = i;
                var mail = person.ScoredEmailAddresses.First().Address;
                var n = $"{person.GivenName} {person.Surname} ({person.Department})";
                var a = new SimpleAction(i.Id, n, () => { });
                a.SecondaryActions.Add(new SimpleAction("Chat", ($"Call {n}"), () =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = $"MSTeams:/l/chat/0/0?users={mail}",
                        UseShellExecute = true
                    });
                }));
                a.SecondaryActions.Add(new SimpleAction("Mail", ($"Mail to {mail}"), () =>
                {
                    var outlook = new Microsoft.Office.Interop.Outlook.Application();
                    var item = (MailItem) outlook.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                    item.To = mail;
                    item.Body = $@"Hallo {person.GivenName},

Freundliche Grüße,

{Me.Result.GivenName}";
                    item.Display(false);
                }));

                a.SecondaryActions.Add(new SimpleAction("Meeting", ($"Schedule meeting with {n}"), () =>
                {
                    var outlook = new Microsoft.Office.Interop.Outlook.Application();
                    var item = (AppointmentItem)outlook.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olAppointmentItem);
                    item.RequiredAttendees = mail;
                    item.Body = $@"Hallo {person.GivenName},

Ziel: 

Freundliche Grüße,

{Me.Result.GivenName}";
                    item.Display(false);
                }));

                a.SecondaryActions.Add(new SimpleAction("Memo", ($"Memo with {n}"), () =>
                {
                    var outlook = new Microsoft.Office.Interop.Outlook.Application();
                    var item = (AppointmentItem)outlook.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olAppointmentItem);
                    item.RequiredAttendees = mail;
                    item.Start = DateTime.Now;
                    item.End = item.Start.AddMinutes(15);
                    item.Subject = $@"{person.Surname} - {Me.Result.GivenName}";
                    item.Save();
                }));
                a.SecondaryActions.Add(new SimpleAction("Markdown", ($"Markdown link for {n}"), () => 
                {
                    var mail = person.ScoredEmailAddresses.First().Address;
                    var md = $"[{person.GivenName} {person.Surname}](mailto:{mail})";
                    query.Context.InsertText(md);
                }));

                return a.ToResult();
            });
        }

        [TestFixture]
        public class Test
        {
            [Test]
            public void Query()
            {
                var s = new People();
                var q = hagen.Query.Parse(new MockContext(), "Andreas");
                var results = s.GetResults(q);
                Console.WriteLine(results.Join());
            }
        }
    }
}
