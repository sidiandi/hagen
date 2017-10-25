// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.CommandLine;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Outlook;
using Sidi.Util;
using Exception = System.Exception;

namespace hagen.plugin.office
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OutlookInspectorIsInForeground : VisibilityConditionAttribute
    {
        public override bool GetIsVisible(IContext context)
        {
            return string.Equals("rctrl_renwnd32", context.GetTopLevelWindowClassName());
        }
    }

    [Usage("Outlook commands")]
    public class Outlook
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TimeSpan appointmentReminderInterval = TimeSpan.FromSeconds(30);
        private TimeSpan appointmentReminderLookahead = TimeSpan.FromMinutes(5);

        readonly IContext _context;
        private DateTime minStartTimeForReminders = DateTime.MinValue;

        public Outlook(IContext context)
        {
            this._context = context;
            showOutlookAppointmentRemindersTimer = new Timer(new TimerCallback(state => ShowOutlookAppointmentsReminder()), null, appointmentReminderInterval, appointmentReminderInterval);
        }

        static DateTime Max(DateTime a, DateTime b)
        {
            if (a > b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }

        private void ShowOutlookAppointmentsReminder()
        {
            try
            {
                // log.Debug("check upcoming appointments");
                // var app = OutlookExtensions.ProvideApplication();
                var app = OutlookExtensions.GetRunningApplication();
                if (app == null)
                {
                    // Outlook is not running - do nothing
                    return;
                }
                var ns = app.GetNamespace("MAPI");
                var calendarFolder = ns.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);

                var i = calendarFolder.Items;
                i.IncludeRecurrences = true;
                i.Sort("[Start]");

                var now = DateTime.Now;
                var startTime = new TimeInterval(now, now + appointmentReminderLookahead);

                // remind all meetings that
                // - start within time span appointmentReminderLookahead in the future
                // - are currently in progress
                // Exclude meetings that
                // - have a start time below minStartTimeForReminders
                // - are not "busy" or "out of office"

                var q = String.Format("([Start] >= {0} And [Start] < {1}) Or ([Start] <= {2} And [End] > {2})",
                    startTime.Begin.OutlookQueryFormat().Quote(), 
                    startTime.End.OutlookQueryFormat().Quote(),
                    now.OutlookQueryFormat().Quote());

                var upcoming = i.Restrict(q).OfType<AppointmentItem>()
                    .Where(a => (a.BusyStatus == OlBusyStatus.olBusy || a.BusyStatus == OlBusyStatus.olOutOfOffice) && 
                    a.Start > minStartTimeForReminders).ToList();

                if (upcoming.Any())
                {
                    var message =
                        upcoming.Select(
                                a =>
                                    String.Format("{0}: {1:HH:mm} {2}", 
                                        HumanreadableRelativeTime(a.Start), 
                                        a.Start,
                                        new[] {
                                        a.Subject, 
                                        a.Location}.Where(_ => !String.IsNullOrEmpty(_)).Join(", ")))
                            .Join();
                    _context.Notify(message);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        static string HumanreadableRelativeTime(DateTime t)
        {
            var r = t - DateTime.Now;
            if (r < TimeSpan.Zero)
            {
                return String.Format("Since {0:F0} minutes", -r.TotalMinutes);
            }
            else
            {
                return String.Format("In {0:F0} minutes", r.TotalMinutes);
            }
        }

        private Timer showOutlookAppointmentRemindersTimer;

        [OutlookInspectorIsInForeground]
        [Usage("Add a task item to Outlook. Use e.g. \"until friday\" or \"in 4 weeks\" to set a due date.")]
        public void Todo(string subject)
        {
            var app = OutlookExtensions.GetRunningApplication();
            var task = app.CreateTaskItem();
            task.Subject = subject;
            var s = new TimeParser().ParseSubject(subject);
            task.DueDate = s.DueDate;
            task.StartDate = DateTime.Now;
            task.Subject = s.Text;
            task.Save();
        }

        [OutlookInspectorIsInForeground]
        [Usage("Invite everyone on the selected mail to a meeting.")]
        public void InviteEveryone()
        {
            var app = OutlookExtensions.ProvideApplication();
            if (app.TryGetSelectedMail(out var mail))
            {
                app.InviteEveryone(mail);
            }
        }

        [OutlookInspectorIsInForeground]
        [Usage("Delegate answering a mail to someone else")]
        public void Delegate()
        {
            var app = OutlookExtensions.ProvideApplication();
            if (app.TryGetSelectedMail(out var mail))
            {
                app.Delegate(mail);
            }
        }

        [Usage("Add a greeting from recipients")]
        public void Hello()
        {
            var app = OutlookExtensions.ProvideApplication();
            var inspector = app.ActiveInspector();
            if (inspector == null) return;

            var item = inspector.CurrentItem as MailItem;
            if (item == null) return;

            inspector.Hello();
        }

        [OutlookInspectorIsInForeground]
        [Usage("Reply to mail")]
        public void ReplyDu()
        {
            var app = OutlookExtensions.ProvideApplication();
            if (app.TryGetSelectedMail(out var mail))
            {
                app.ReplyDu(mail);
            }
        }

        [Usage("Dismiss reminders for started appointments")]
        public void DismissReminders()
        {
            minStartTimeForReminders = DateTime.Now;
        }
    }
}
