// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.CommandLine;
using System.Runtime.InteropServices;

namespace hagen.plugin.office
{
    [Usage("Outlook commands")]
    class Outlook
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Outlook()
        {
        }

        [Usage("Add a task item to Outlook")]
        public void Task(string subject)
        {
            Todo(subject);
        }

        [Usage("Add a To Do item to Outlook")]
        public void Todo(string subject)
        {
            var app = OutlookExtensions.ProvideApplication();
            var task = app.CreateTaskItem();
            task.Subject = subject;
            task.DueDate = DateTime.Today.AddDays(1);
            task.Save();
        }
    }
}
