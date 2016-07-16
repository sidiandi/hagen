using NUnit.Framework;
using Sidi;
using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.Tests
{
    [TestFixture()]
    public class RunOnLogonTests
    {
        [Test()]
        public void GetTest()
        {
            var path = new LPath(@"C:\temp\test.exe");
            RunOnLogon.Set(path, true);
            Assert.IsTrue(RunOnLogon.Get(path));
            RunOnLogon.Set(path, false);
            Assert.IsFalse(RunOnLogon.Get(path));
        }
    }
}