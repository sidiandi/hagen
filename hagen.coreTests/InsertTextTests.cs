using NUnit.Framework;
using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.IO;

namespace hagen.Tests
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