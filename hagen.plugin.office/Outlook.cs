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

using Sidi.Util;
using Exception = System.Exception;
using NetOffice.OutlookApi;

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
    public class Outlook : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly IContext _context;
        public Outlook(IContext context)
        {
            this._context = context;
            this.appointmentReminder = new AppointmentReminder(context);
            this.worktimeAlert = new WorktimeAlert(context);
        }

        readonly AppointmentReminder appointmentReminder;
        private readonly WorktimeAlert worktimeAlert;

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
            appointmentReminder.Dismiss();
            worktimeAlert.Dismiss();
        }

        public void Dispose()
        {
            appointmentReminder.Dispose();
            worktimeAlert.Dispose();
        }
    }
}
