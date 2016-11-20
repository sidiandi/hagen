using NUnit.Framework;
using hagen.plugin.office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Test;
using Microsoft.Office.Interop.Outlook;
using Sidi.Extensions;

namespace hagen.plugin.office.Tests
{
    [TestFixture()]
    public class OutlookExtensionsTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test(), Explicit("requires Outlook")]
        public void BetterDecline()
        {
            var app = OutlookExtensions.ProvideApplication();
            var explorer = app.ActiveExplorer();
            log.Info(explorer.Caption);

            var declinedFolder = app.ProvideFolder(OlDefaultFolders.olFolderCalendar, "declined");

            foreach (var i in explorer.Selection.OfType<AppointmentItem>())
            {
                log.Info(i.Subject);
                i.Respond(OlMeetingResponse.olMeetingDeclined);
                i.Move(declinedFolder);
            }
        }

        [Test(), Explicit("requires outlook")]
        public void ProvideFolderTest()
        {
            var app = OutlookExtensions.ProvideApplication();
            var f1 = app.ProvideFolder(OlDefaultFolders.olFolderCalendar, "test");
            log.Info(app.GetSession().GetDefaultFolder(OlDefaultFolders.olFolderCalendar).Folders.OfType<MAPIFolder>().Select(_ => _.Name).Join());

            var f2 = app.ProvideFolder(OlDefaultFolders.olFolderCalendar, "test");
            log.Info(app.GetSession().GetDefaultFolder(OlDefaultFolders.olFolderCalendar).Folders.OfType<MAPIFolder>().Select(_ => _.Name).Join());

            Assert.AreEqual(f1.Name, f2.Name);
            log.Info(app.GetSession().GetDefaultFolder(OlDefaultFolders.olFolderCalendar).Folders.OfType<MAPIFolder>().Select(_ => _.Name).Join());
        }
    }
}