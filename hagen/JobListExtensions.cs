using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Forms;
using System.ComponentModel;

namespace hagen
{
    public static class JobListExtensions
    {
        public static void AddJob(this JobList jobList, string name, System.Action action)
        {
            jobList.AddJob(name, bgw =>
            {
                using (new BackgroundWorkerReportProgressAppender(bgw))
                {
                    action();
                }
            });
        }
    }
}
