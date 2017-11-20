using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;

namespace hagen
{
    [TestFixture()]
    public class MarkdownNotesReaderTests : TestBase
    {
        [Test()]
        public void ReadTest()
        {
            // arrange
            var markdownFile = this.TestFile("notes.md");

            // act
            var notes = MarkdownNotesReader.Read(markdownFile).ToArray();

            // assert
            Assert.AreEqual(3, notes.Length);
            var n = notes[2];
            Assert.AreEqual("Birthday < Donald Duck < Person", n.Name);
            Assert.AreEqual("1.1.2000", n.Content);
        }
    }
}