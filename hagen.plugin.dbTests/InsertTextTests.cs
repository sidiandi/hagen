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
    public class InsertTextTests
    {
        public void ReadSections()
        {
            var f = Paths.BinDir.CatDir(@"test\sections.txt");
            var s = InsertText.ReadSections(f);
            Assert.AreEqual(3, s.Count);
            Assert.AreEqual("This is text A.", s["A"]);
        }
    }
}