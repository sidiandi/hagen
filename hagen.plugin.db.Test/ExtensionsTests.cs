using NUnit.Framework;
using hagen.Plugin.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.IO;

namespace hagen.Plugin.Db.Tests
{
    [TestFixture()]
    public class ExtensionsTests
    {
        [Test()]
        public void FileNameWithContextTest()
        {
            Assert.AreEqual("c.txt < b < a", new LPath(@"C:\a\b\c.txt").FileNameWithContext());
        }

        [Test()]
        public void FileNameWithContextTestUnc()
        {
            Assert.AreEqual("c.txt < b < a < share", new LPath(@"\\example.com\share\a\b\c.txt").FileNameWithContext());
        }

        [Test()]
        public void FileNameWithContextTestUnc2()
        {
            Assert.AreEqual("share < example.com", new LPath(@"\\example.com\share").FileNameWithContext());
        }
    }
}
