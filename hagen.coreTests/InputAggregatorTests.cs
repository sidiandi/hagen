using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace hagen.Tests
{
    [TestFixture()]
    public class InputAggregatorTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit]
        public void TestAggregate()
        {
            var a = new InputAggregator();

            using (var hidMonitor = new HumanInterfaceDeviceMonitor())
            using (new System.Reactive.Disposables.CompositeDisposable(
                ObservableTimeInterval.GetSeconds().Take(10).Subscribe(a.Time),
                hidMonitor.Mouse.Subscribe(a.Mouse),
                hidMonitor.KeyDown.Subscribe(a.KeyDown),
                hidMonitor.Mouse.ToDistance().Subscribe(_ => log.Info(_)),
                a.Input.Subscribe(_ => log.Info(_.MouseMove))
                ))
            {
                a.Input.Wait();
            }
        }
    }
}