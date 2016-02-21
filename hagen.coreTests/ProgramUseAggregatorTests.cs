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
    public class ProgramUseAggregatorTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit]
        public void TestAggregate()
        {
            var a = new ProgramUseAggregator();

            Observable.Interval(TimeSpan.FromMilliseconds(25)).Select(_ => new KeyEventArgs(Keys.A)).Subscribe(a.KeyDown);

            using (new WinEventHook().ForegroundWindowChanged.Subscribe(a.Window))
            using (var intercept = new HumanInterfaceDeviceMonitor())
            {
                intercept.Mouse.Subscribe(a.Mouse);
                intercept.KeyDown.Subscribe(a.KeyDown);
                a.ProgramUse.Subscribe(_ => log.Info(_.Details()));
                ObservableTimeInterval.GetSeconds().Take(10).Wait();
            }
        }
    }
}