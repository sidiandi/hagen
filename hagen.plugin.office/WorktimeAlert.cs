using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagen
{
    class WorktimeAlert
    {
        public WorktimeAlert(IContext context)
        {
            this.context = context;
            workTime = context.GetService<IWorkTime>();
        }

        IContract Contract => workTime.Contract;
        IWorkTime workTime;

        System.Windows.Forms.Timer alertTimer = null;
        TimeSpan alertInterval = TimeSpan.FromMinutes(5);
        TimeSpan warnBefore = TimeSpan.FromHours(1);
        TimeSpan warnAfter = TimeSpan.FromMinutes(30);
        private readonly IContext context;

        void StartWorkTimeAlert()
        {
            alertTimer = new System.Windows.Forms.Timer()
            {
                Interval = (int)alertInterval.TotalMilliseconds
            };
            alertTimer.Tick += new EventHandler((s, e) =>
            {
                CheckWorkTime();
            });
            alertTimer.Start();
        }

        void CheckWorkTime()
        {
            var now = DateTime.Now;
            var begin = workTime.GetWorkBegin(now);
            if (begin == null)
            {
                return;
            }

            var mustGo = begin.Value + Contract.MaxWorkTimePerDay;
            var warn = new TimeInterval(mustGo - warnBefore, mustGo + warnAfter);

            if (warn.Contains(now))
            {
                ShowWorkTimeAlert();
            }
        }

        public void ShowWorkTimeAlert()
        {
            var now = DateTime.Now;
            var begin = workTime.GetWorkBegin(now);
            string text;
            if (begin == null)
            {
                begin = DateTime.Now;
            }

            {
                var mustGo = begin.Value + Contract.MaxWorkTimePerDay;
                var go = begin.Value + (Contract.RegularWorkTimePerDay + Contract.PauseTimePerDay);

                text = String.Format(
    @"Go: {5:HH:mm:ss}
Latest go: {2:HH:mm:ss}
Time left: {4:hh\:mm}

Current: {3:HH:mm:ss}
Come: {1:HH:mm:ss}
Hours: {0:G3}",
                    (now - begin.Value).TotalHours,
                    begin.Value,
                    mustGo,
                    now,
                    mustGo - now,
                    go);
            }
        
            context.Notify(text);
        }
    }
}
