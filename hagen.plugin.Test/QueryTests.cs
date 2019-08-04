using NUnit.Framework;
using Sidi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.plugin.Tests
{
    [TestFixture()]
    public class QueryTests
    {
        [Test()]
        public void ParseTest()
        {
            var q = Query.Parse(new ContextMock(), "#tag1 #tag2 hello world");
            Assert.AreEqual(2, q.Tags.Count());
            Assert.IsTrue(q.Tags.Contains("#tag1"));
            Assert.IsTrue(q.Tags.Contains("#tag2"));
            Assert.AreEqual("hello world", q.Text);
            Assert.AreEqual(12, q.TextBegin);
        }
    }
}