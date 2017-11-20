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
    public class WinEventHookTests : Sidi.Test.TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit("interactive, runs 60 seconds")]
        public void ShowMessages()
        {
            using (var wih = new WinEventHook())
            {
                wih.ForegroundWindowChanged.Subscribe(_ => Console.WriteLine(_));
                Thread.Sleep(TimeSpan.FromSeconds(60));

            }
        }
    }
}