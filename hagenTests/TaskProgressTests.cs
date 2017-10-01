using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace hagen.Tests
{
    [TestFixture()]
    public class TaskProgressTests
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Apartment(ApartmentState.STA), Explicit("does not run on Jenkins")]
        public void Run()
        {
            var cs = new CancellationTokenSource();

            var t = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100 && !cs.Token.IsCancellationRequested; ++i)
                {
                    log.Info(i);
                    Thread.Sleep(100);
                }
            }, cs.Token);
            var p = new TaskProgress(t, cs);

            p.ShowDialog();
        }
    }
}