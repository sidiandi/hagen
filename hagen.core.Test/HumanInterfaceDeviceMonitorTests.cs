using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace hagen.Tests
{
    [TestFixture()]
    public class HumanInterfaceDeviceMonitorTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit("interactive")]
        public void Capture()
        {
            using (var ih = new HumanInterfaceDeviceMonitor())
            {
                using (ih.Mouse.Subscribe(_ => log.InfoFormat("{0} {1}", _.Location, _.Clicks)))
                {
                    ih.KeyDown.TakeWhile(_ => _.KeyCode != Keys.Escape).Wait();
                }
            }
        }
    }
}