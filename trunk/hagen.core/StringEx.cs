using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom;
using System.IO;
using NUnit.Framework;

namespace hagen
{
    public static class StringEx
    {
        public static string EscapeCsharpStringLiteral(this string input)
        {
            var provider = new CSharpCodeProvider();
            var writer = new StringWriter();
            provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
            var quoted = writer.ToString();
            return quoted.Substring(1, quoted.Length - 2);
        }

        [TestFixture]
        public class Test
        {
            [Test]
            public void Escape()
            {
                Assert.AreEqual(@"\\", @"\".EscapeCsharpStringLiteral());
                Assert.AreEqual(@".EscapeCsharpStringLiteral()", ".EscapeCsharpStringLiteral()".EscapeCsharpStringLiteral());
            }
        }
    }
}
