using hagen.plugin.office;
using NetOffice.OutlookApi;
using NetOffice.OutlookApi.Enums;
using Sidi.Extensions;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hagen.plugin.office
{
    sealed class AppointmentReminder : IDisposable
    {
        readonly IContext _context;

        public AppointmentReminder(IContext context)
        {
            checkTimer = new Timer(new TimerCallback(state => ShowOutlookAppointmentsReminder()), null, TimeSpan.FromSeconds(1), checkInterval);
            _context = context;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Timer checkTimer;
        private TimeSpan checkInterval = TimeSpan.FromSeconds(30);
        private DateTime minStartTimeForReminders = DateTime.MinValue;
        private TimeSpan lookahead = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Dismiss currently shown reminders
        /// </summary>
        public void Dismiss()
        {
            minStartTimeForReminders = DateTime.Now + lookahead;
        }

        private void ShowOutlookAppointmentsReminder()
        {
            try
            {
                var app = OutlookExtensions.GetRunningApplication();
                if (app == null)
                {
                    // Outlook is not running - do nothing
                    return;
                }

                var calendarFolder = app.GetCalendar();

                var upcoming = GetUpcomingAppointments(calendarFolder, lookahead, minStartTimeForReminders);

                if (upcoming.Any())
                {
                    var message = upcoming.Select(GetReminder).Join();
                    _context.Notify(message);
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        static IList<AppointmentItem> GetUpcomingAppointments(
            MAPIFolder calendarFolder, 
            TimeSpan appointmentReminderLookahead, 
            DateTime minStartTimeForReminders)
        {
                var now = DateTime.Now;
                var startTime = new TimeInterval(now, now + appointmentReminderLookahead);
                var upcoming = calendarFolder.GetOutlookAppointmentsActiveIn(startTime)
                    .Where(a => 
                        (a.BusyStatus == OlBusyStatus.olBusy || a.BusyStatus == OlBusyStatus.olOutOfOffice) && 
                        (a.Start > minStartTimeForReminders)
                    ).ToList();

            return upcoming;
        }

        static string GetReminder(AppointmentItem a) => $"{a.Start.GetHumanreadableRelativeTime()}: {a.Start:HH:mm} {new[] { a.Subject, a.Location }.Where(_ => !String.IsNullOrEmpty(_)).Join(", ")}";

        public void Dispose()
        {
            checkTimer.Dispose();
        }
    }
}
