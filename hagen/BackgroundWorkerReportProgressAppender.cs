using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using System.ComponentModel;
using NUnit.Framework;

namespace hagen
{
    class BackgroundWorkerReportProgressAppender : AppenderSkeleton, IDisposable
    {
        public BackgroundWorkerReportProgressAppender(BackgroundWorker bgw)
        {
            this.backgroundWorker = bgw;
            var hierarchy = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository());
            hierarchy.Root.AddAppender(this);
            this.Layout = new log4net.Layout.SimpleLayout();
            this.ActivateOptions();
        }

        BackgroundWorker backgroundWorker;
        int logCount = 0;

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            ++logCount;
            backgroundWorker.ReportProgress(logCount, this.RenderLoggingEvent(loggingEvent));
        }

        public void Dispose()
        {
            var hierarchy = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository());
            hierarchy.Root.RemoveAppender(this);
        }

        [TestFixture]
        public class Test : Sidi.Test.TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            [Test]
            public void Log()
            {
                int p = 0;

                var bgw = new BackgroundWorker()
                {
                    WorkerReportsProgress = true
                };
                bgw.ProgressChanged += (s, e) =>
                    {
                        p = e.ProgressPercentage;
                    };
                using (new BackgroundWorkerReportProgressAppender(bgw))
                {
                    log.Info("hello");
                }

                Assert.AreEqual(1, p);
            }
        }
    }
}
