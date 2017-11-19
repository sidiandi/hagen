using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;
using Sidi.Forms;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using System.IO;
using Sidi.Util;
using System.Drawing;
using Sidi.Extensions;
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
            // https://www.ietf.org/rfc/rfc3339.txt
            Layout = new PatternLayout("%date{yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz} %level %logger %ndc - %message%newline%exception")
            {
                IgnoresException = false
            };

            textView = new ScintillaNET.Scintilla()
            {
                Dock = DockStyle.Fill,
                // IsReadOnly = true,
                //Font = new Font(FontFamily.GenericMonospace, 10.0f),
            };

            textView.StyleNeeded += (s, e) =>
                {
                    // e.Range.SetStyle(16);
                };

            textView.Font = new Font("Courier New", 10);

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
                    textView.CurrentPos = textView.TextLength;
                    output.Dispose();
                    output = null;
                }
            }
        }

        StringWriter output;
        System.Windows.Forms.Timer timer;

        ScintillaNET.Scintilla textView;

        public void Close()
        {
        }

        public new ILayout Layout { get; set; }

        public void DoAppend(log4net.Core.LoggingEvent loggingEvent)
        {
            if (loggingEvent.Level.CompareTo(Threshold) >= 0)
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

        public log4net.Core.Level Threshold {get; set; }
    }
}
