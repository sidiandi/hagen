// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using Sidi.Util;

namespace hagen.plugin.office
{
    public static class OutlookExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Application ProvideApplication()
        {
            try
            {
                var instance = Marshal.GetActiveObject("Outlook.Application") as Microsoft.Office.Interop.Outlook.Application;
                if (instance != null)
                {
                    return instance;
                }
            }
            catch
            {
            }

            return new Microsoft.Office.Interop.Outlook.Application();
        }

        public static TaskItem CreateTaskItem(this Application app)
        {
            var task = (Microsoft.Office.Interop.Outlook.TaskItem)app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olTaskItem);
            return task;
        }

        public static NameSpace GetSession(this Application app)
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

        public static IList<AppointmentItem> GetOutlookAppointmentsStartingIn(this Application outlook, MAPIFolder calendarFolder, TimeInterval time)
        {
            var q = String.Format("[Start] >= {0} And [Start] < {1}", time.Begin.OutlookQueryFormat().Quote(), time.End.OutlookQueryFormat().Quote());
            var i = calendarFolder.Items;
            i.IncludeRecurrences = true;
            i.Sort("[Start]");
            var appointments = i.Restrict(q);
            return appointments.OfType<AppointmentItem>().ToList();
        }
    }
}
