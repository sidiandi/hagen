using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using log4net.Appender;
using log4net.Core;
using Sidi.Forms;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using System.IO;
using Sidi.Util;
using System.Drawing;
using Sidi.Extensions;
using BrightIdeasSoftware;
using Sidi.Test;

namespace Sidi.Forms
{
    public class LogViewer2 : Control, IAppender
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void AddToRoot()
        {
            ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()).Root.AddAppender(this);
        }

        public LogViewer2()
        {
            Layout = new PatternLayout("%utcdate{ISO8601} %level %logger %ndc - %message%newline%exception")
            {
                IgnoresException = false
            };

            textView = new ScintillaNET.Scintilla()
            {
                Dock = DockStyle.Fill,
                // IsReadOnly = true,
                //Font = new Font(FontFamily.GenericMonospace, 10.0f),
            };

            this.Controls.Add(textView);

            timer = new Timer()
            {
                Interval = 1000
            };
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            
            log.Info("started");
        }

        void timer_Tick(object sender, EventArgs e)
        {
            lock (this)
            {
                if (output != null)
                {
                    textView.AppendText(output.ToString());
                    output.Dispose();
                    output = null;
                }
            }
        }

        StringWriter output;
        System.Windows.Forms.Timer timer;

        ScintillaNET.Scintilla textView;

        [TestFixture]
        public class Test : TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            [Test, RequiresSTA]
            public void List()
            {
                var lv = new LogViewer2();
                lv.AddToRoot();
                log.Error("error");
                log.Warn("warn");
                var form = lv.AsForm("Log Viewer");
                form.Show();
                foreach (var i in Enumerable.Range(0, 100))
                {
                    log.Info(i);
                }

                log4net.ThreadContext.Stacks["NDC"].Push("hello");

                using (new LogScope(log.Info, "hello"))
                {
                }

                try
                {
                    String s = null;
                    Console.WriteLine(s.Length);
                }
                catch (Exception ex)
                {
                    log.Warn("some error", ex);
                }

                Application.Run(form);
            }

        }

        public void Close()
        {
        }

        public new ILayout Layout { get; set; }

        public void DoAppend(log4net.Core.LoggingEvent loggingEvent)
        {
            lock (this)
            {
                if (output == null)
                {
                    output = new StringWriter();
                }
                Layout.Format(output, loggingEvent);
            }
        }
    }
}
