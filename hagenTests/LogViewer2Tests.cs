using NUnit.Framework;
using Sidi.Forms;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sidi.Forms.Tests
{
    [TestFixture()]
    public class LogViewer2Tests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Apartment(System.Threading.ApartmentState.STA)]
        public void List()
        {
            var lv = new LogViewer2()
            {
                Size = new Size(800, 400)
            };
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
}