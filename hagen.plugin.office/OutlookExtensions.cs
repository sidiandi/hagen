// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using System.Runtime.InteropServices;

using Sidi.Util;
using System.Text.RegularExpressions;
using NetOffice.OutlookApi;
using NetOffice.OutlookApi.Enums;

namespace hagen.plugin.office
{
    public static class OutlookExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Application ProvideApplication()
        {
            var instance = Application.GetActiveInstance();
            if (instance != null)
            {
                return instance;
            }
            return new Application();
        }

        /// <summary>
        /// Running outlook instance or null
        /// </summary>
        /// <returns></returns>
        public static Application GetRunningApplication()
        {
            return Application.GetActiveInstance();
        }

        public static TaskItem CreateTaskItem(this Application app)
        {
            return (TaskItem)app.CreateItem(NetOffice.OutlookApi.Enums.OlItemType.olTaskItem);
        }

        public static _NameSpace GetSession(this Application app)
        {
            return app.ActiveExplorer().Session;
        }

        public static MAPIFolder ProvideFolder(this Application app, OlDefaultFolders root, string name)
        {
            var rootFolder = (MAPIFolder) app.GetSession().GetDefaultFolder(root);
            var existingFolder = rootFolder.Folders.OfType<MAPIFolder>().FirstOrDefault(_ => object.Equals(_.Name, name));
            if (existingFolder != null)
            {
                return existingFolder;
            }

            var newFolder = rootFolder.Folders.Add(name, System.Reflection.Missing.Value);
            return newFolder;
        }

        public static MAPIFolder GetCalendar(this Application application)
        {
            var ns = application.GetNamespace("MAPI");
            var calendarFolder = ns.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
            return calendarFolder;
        }

        public static MAPIFolder GetCalendar(this Application application, string calendarName)
        {
            var ns = application.GetNamespace("MAPI");
            var calendarFolder = ns.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
            try
            {
                return calendarFolder.Folders[calendarName];
            }
            catch (System.Exception)
            {
                throw new ArgumentOutOfRangeException("calendarName", calendarName, String.Format("Calendar {0} not found. Available: {1}", calendarName, calendarFolder.Items));
            }
        }

        public static string OutlookQueryFormat(this DateTime t)
        {
            // "October 24, 2002 12:00 AM" 
            // return t.ToString("MMMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture);
            return t.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        public static IList<AppointmentItem> GetOutlookAppointments(this Application outlook, MAPIFolder calendarFolder, TimeInterval time)
        {
            calendarFolder.Items.Sort("[Start]", false);
            calendarFolder.Items.IncludeRecurrences = true;
            var q = String.Format("[Start] >= {0} And [End] < {1}", time.Begin.OutlookQueryFormat().Quote(), time.End.OutlookQueryFormat().Quote());
            log.Info(q);
            var appointments = calendarFolder.Items.Restrict(q);
            return appointments.OfType<AppointmentItem>().ToList();
        }

        public static IList<AppointmentItem> GetOutlookAppointmentsActiveIn(this MAPIFolder calendarFolder, TimeInterval time)
        {
            calendarFolder.Items.Sort("[Start]", false);
            calendarFolder.Items.IncludeRecurrences = true;
            var q = String.Format($"[Start] <= {time.End.OutlookQueryFormat().Quote()} And [End] > {time.End.OutlookQueryFormat().Quote()}");
            log.Info(q);
            var appointments = calendarFolder.Items.Restrict(q);
            return appointments.OfType<AppointmentItem>().ToList();
        }

        public static IList<AppointmentItem> GetOutlookAppointmentsStartingIn(this Application outlook, MAPIFolder calendarFolder, TimeInterval time)
        {
            var q = String.Format("[Start] >= {0} And [Start] < {1}", time.Begin.OutlookQueryFormat().Quote(), time.End.OutlookQueryFormat().Quote());
            var i = calendarFolder.Items;
            i.IncludeRecurrences = true;
            i.Sort("[Start]");
            var appointments = i.Restrict(q);
            return appointments.OfType<AppointmentItem>().ToList();
        }

        public static bool TryGetSelectedMail(this Application outlook, out MailItem mail)
        {
            var explorer = outlook.ActiveExplorer();
            if (explorer.Selection.Count > 0)
            {
                mail = explorer.Selection[1] as MailItem;
                return mail != null;
            }

            mail = null;
            return false;
        }

        class InspectorWrapper
        {
            readonly Inspector inspector;
            readonly object item;

            public static InspectorWrapper KeepAliveUntilClose(Inspector inspector)
            {
                return new InspectorWrapper(inspector);
            }

            InspectorWrapper(Inspector inspector)
            {
                this.inspector = inspector;
                this.item = inspector.CurrentItem;
                trackedInspectors.Add(this);
                inspector.CloseEvent += InspectorWrapper_Close;
            }

            private void InspectorWrapper_Close()
            {
                trackedInspectors.Remove(this);
                inspector.CloseEvent -= InspectorWrapper_Close;
            }

            static List<InspectorWrapper> trackedInspectors = new List<InspectorWrapper>();
        }

        /// <summary>
        /// Forward mail for delegation. All (To) recipients of the original mail and the sender will be in (CC).
        /// </summary>
        /// <param name="outlook"></param>
        /// <param name="mail"></param>
        public static void Delegate(this Application outlook, MailItem mail)
        {
            var forwardMail = mail.Forward();
            forwardMail.Body = @"Hallo,

bitte beantworten.

Freundliche Grüße,

Andreas" + forwardMail.Body;

            var ownAddress = outlook.Session.CurrentUser.Address;
            foreach (Recipient r in mail.Recipients)
            {
                if (r.Type == (int) OlMailRecipientType.olTo && !(string.Equals(ownAddress, r.Address)))
                {
                    var nr = forwardMail.Recipients.Add(r);
                    nr.Type = (int) OlMailRecipientType.olCC;
                }
            }

            {
                var originalSender = forwardMail.Recipients.Add(mail.Sender);
                originalSender.Type = (int)OlMailRecipientType.olCC;
            }

            var inspector = (Inspector)outlook.Inspectors.Add(forwardMail);
            inspector.Activate();
        }

        public static void Hello(this _Inspector inspector)
        {
            var mailItem = (MailItem) inspector.CurrentItem;

            var names = mailItem.Recipients.Cast<Recipient>()
                .Where(_ => _.Type == (int)OlMailRecipientType.olTo)
                .Select(_ => { _.AddressEntry.TryGetGreetingName(out var name); return name; })
                .Where(_ => _ != null);

            var greeting = String.Format(@"Hallo {0},

text

Freundliche Grüße,

Andreas
"
                , names.Join(", "));

            dynamic editor = inspector.WordEditor;
            dynamic selection = editor.Windows[1].Selection;
            selection.Delete();
            selection.InsertAfter(greeting);
        }

        public static void ReplyDu(this Application outlook, MailItem mail)
        {
            var replyMail = mail.Reply();
            if (mail.Sender.TryGetGreetingName(out var greetingName))
            {
                replyMail.Body = String.Format(@"Hallo {0},



Freundliche Grüße,

Andreas
", greetingName) + replyMail.Body;
            }

            var inspector = (Inspector)outlook.Inspectors.Add(replyMail);
            inspector.Activate();
        }

        public static Recipient Add(this Recipients recipients, Recipient recipientToAdd)
        {
            var r = recipients.Add(recipientToAdd.Name);
            r.Type = recipientToAdd.Type;
            r.AddressEntry = recipientToAdd.AddressEntry;
            return r;
        }

        public static Recipient Add(this Recipients recipients, AddressEntry addressEntry)
        {
            var r = recipients.Add(addressEntry.Name);
            r.AddressEntry = addressEntry;
            return r;
        }

        public static bool TryGetGreetingName(this AddressEntry addressEntry, out string greetingName)
        {
            var displayName = addressEntry.Name;

            // lastname, first names
            var p = Regex.Split(displayName, @",\s+");
            if (p.Length > 1)
            {
                greetingName = p[1];
                return true;
            }

            // firtnames lastname
            p = Regex.Split(displayName, @"\s+");
            if (p.Length > 0)
            {
                greetingName = p[0];
                return true;
            }

            greetingName = null;
            return false;
        }

        public static void InviteEveryone(this Application outlook, MailItem mail)
        {
            var appointment = outlook.CreateItem(OlItemType.olAppointmentItem) as AppointmentItem;
            appointment.Start = DateTime.Now.NextFullHour();
            appointment.End = appointment.Start.AddHours(0.5);
            appointment.Subject = mail.Subject;
            appointment.Attachments.Add(mail);
            appointment.MeetingStatus = OlMeetingStatus.olMeeting;

            // add to and cc from mail as recipients
            foreach (Recipient recipient in mail.Recipients)
            {
                var addedRecipient = appointment.Recipients.Add(recipient.Name);
                addedRecipient.Type = (int) OlMeetingRecipientType.olRequired;
                addedRecipient.AddressEntry = recipient.AddressEntry;
            }

            // add mail sender as attendee
            {
                var addedRecipient = appointment.Recipients.Add(mail.Sender.Name);
                addedRecipient.AddressEntry = mail.Sender;
                addedRecipient.Type = (int)OlMeetingRecipientType.olRequired;
            }

            // Remove myself
            var recipients = appointment.Recipients;
            for (int i=1; i<appointment.Recipients.Count; ++i)
            {
                log.Info(recipients[i].Name);
            }

            var inspector = (Inspector)outlook.Inspectors.Add(appointment);
            inspector.Activate();
        }
    }
}
