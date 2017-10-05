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
    public class MultiWordMatchTests
    {
        [Test()]
        public void IsMatchTest()
        {
            var q = new MultiWordMatch("Hello World");
            Assert.IsFalse(q.IsMatch("Hello"));
            Assert.IsTrue(q.IsMatch("World Hello"));
            Assert.IsTrue(q.IsMatch("world hello"));
        }
    }
}