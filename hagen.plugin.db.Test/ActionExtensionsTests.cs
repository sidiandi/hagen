using NUnit.Framework;
using hagen.Plugin.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Persistence;

namespace hagen.Plugin.Db.Tests
{
    [TestFixture()]
    public class ActionExtensionsTests : Sidi.Test.TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit("Internet explorer must be open")]
        public void Links()
        {
            foreach (var a in ActionExtensions.GetAllIeLinks())
            {
                Console.WriteLine(a);
            }
        }

        [Test, Explicit("fix later")]
        public void AddOrUpdate()
        {
            var dbPath = TestFile("test_actions.sqlite");
            dbPath.EnsureFileNotExists();
            using (var actions = new Collection<Action>(dbPath))
            {
                var f = new FileActionFactory();
                var a0 = f.FromFile(dbPath);

                actions.AddOrUpdate(a0);
                Assert.AreEqual(1, actions.Count);

                var a1 = f.FromFile(dbPath);
                actions.AddOrUpdate(a1);
                Assert.AreEqual(1, actions.Count);
            }
        }
    }
}