using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Persistence;
using NUnit.Framework;

namespace hagen
{
    class LastExecutedDbStore : ILastExecutedStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly Sidi.Persistence.Dictionary<string, DateTime> data;
        readonly System.Collections.Generic.Dictionary<string, DateTime> cache = new System.Collections.Generic.Dictionary<string, DateTime>();

        public LastExecutedDbStore(Sidi.IO.LPath dbPath, string tableName)
        {
            data = new Sidi.Persistence.Dictionary<string, DateTime>(dbPath, tableName);
        }

        public DateTime Get(string id)
        {
            DateTime time;
            if (!cache.TryGetValue(id, out time))
            {
                time = GetUncached(id);
                cache[id] = time;
            }
            return time;
        }

        DateTime GetUncached(string id)
        {
            DateTime time;
            if (data.TryGetValue(id, out time))
            {
                log.InfoFormat("{0} last executed {1}", id, time);
                return time;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public void Set(string id)
        {
            var time = DateTime.UtcNow;
            log.InfoFormat("{0} last executed {1}", id, time);
            data[id] = time;
            cache.Remove(id);
        }

        [TestFixture]
        public class LastExecutedDbStoreTest : Sidi.Test.TestBase
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
}
