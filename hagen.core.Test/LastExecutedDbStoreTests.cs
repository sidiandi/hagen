using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.Tests
{
    [TestFixture()]
    public class LastExecutedDbStoreTests : Sidi.Test.TestBase
    {
        [Test]
        public void ReadWriteTime()
        {
            var d = new Sidi.Persistence.Dictionary<string, DateTime>(TestFile("LastExecutedDbStoreTest.sqlite"), "LastExecuted");
            var id = "IdOfAction";
            var time = DateTime.UtcNow;
            d[id] = time;
            Assert.AreEqual(time, d[id]);
        }
    }
}