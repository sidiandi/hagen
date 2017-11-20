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
    public class StringExtensionsTests
    {
        [Test]
        public void Escape()
        {
            Assert.AreEqual(@"\\", @"\".EscapeCsharpStringLiteral());
            Assert.AreEqual(@".EscapeCsharpStringLiteral()", ".EscapeCsharpStringLiteral()".EscapeCsharpStringLiteral());
        }
    }
}